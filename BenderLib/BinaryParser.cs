namespace BenderLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Reads a binary file and formats the data as specified in the SpecFile
    /// </summary>
    public class BinaryParser
    {
        private readonly SpecFile _mSpec;

        /// <summary>
        /// Creates a new parser
        /// </summary>
        /// <param name="spec">Binary layout spec</param>
        public BinaryParser(SpecFile spec)
        {
            _mSpec = spec;
        }

        /// <summary>
        /// Perform the parse operation against this binary
        /// </summary>
        /// <param name="binary">Data to parse</param>
        /// <returns>Parsed result</returns>
        /// <exception cref="ParseException">Raised if data from offset does not have enough bytes to make a
        /// number with width bytes</exception>
        public Bender Parse(DataFile binary)
        {
            if (binary == null)
            {
                throw new ArgumentException("{0} cannot be null", nameof(binary));
            }

            if (binary.Empty)
            {
                throw new ArgumentException("{0} cannot be empty", nameof(binary));
            }

            try
            {
                return TryParse(binary);
            }
            catch (OutOfDataException ex)
            {
                throw ex as ParseException;
            }
        }

        private Bender TryParse(DataFile binary)
        {
            var bender = new Bender();

            using (var stream = new MemoryStream(binary.Data))
            using (var reader = new BinaryReader(stream))
            {

                foreach (var str in _mSpec.Layout)
                {
                    var els = new List<Element>();
                    var st = _mSpec.Structures.FirstOrDefault(o => o.Name.Equals(str));
                    if (st != null)
                    {
                        els.AddRange(st.Elements);
                    }
                    else
                    {
                        var el = _mSpec.Elements.FirstOrDefault(o => o.Name.Equals(str));
                        if (el != null)
                        {
                            els.Add(el);
                        }
                    }

                    if (els.Count == 0)
                    {
                        var formatted = new Bender.FormattedField
                        {
                            Name = str,
                            Value = new List<string> { "Undefined object" }
                        };
                        bender.FormattedFields.Add(formatted);
                    }
                    else
                    {
                        foreach (var el in els)
                        {
                            var formatted = HandleElement(el, reader, binary);
                            bender.FormattedFields.Add(formatted);
                        }
                    }
                }
            }

            return bender;
        }

        private Bender.FormattedField HandleElement(Element el, BinaryReader reader, DataFile binary)
        {
            var buff = reader.ReadBytes(el.Units);

            // If byte order does not match, flip now
            if (!(el.LittleEndian && BitConverter.IsLittleEndian))
            {
                Array.Reverse(buff);
            }

            // If this is deferred read, collect the deferred data
            if (string.IsNullOrEmpty(el.Deferred))
            {
                return FormatBuffer(el, buff);
            }
            buff = HandleDeferredRead(el, binary, buff);

            if (buff == null)
            {
                return new Bender.FormattedField
                {
                    Name = el.Name,
                    Value = new List<string> { "Error: Invalid deferred object" }
                };
            }

            return FormatBuffer(el, buff);
        }

        /// <summary>
        /// Use buff data to locate a deferred record. If the record can be located,
        /// it will be parsed according the element rules and the raw byte[] will be
        /// returned for further processing.
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="binary">Binary source file</param>
        /// <param name="buff">Buffer record</param>
        /// <returns>Deferred data block</returns>
        private byte[] HandleDeferredRead(Element el, DataFile binary, byte[] buff)
        {
            if (_mSpec.Deferreds == null)
            {
                return null;
            }

            var def = _mSpec.Deferreds.FirstOrDefault(d => d.Name != null && d.Name.Equals(el.Deferred));
            if (def == null)
            {
                return null;
            }

            var size = Number.From(def.SizeUnits, false, 0, buff);
            var offset = Number.From(def.OffsetUnits, false, size.si, buff);

            using (var stream = new MemoryStream(binary.Data))
            using (var reader = new BinaryReader(stream))
            {
                el.Units = size.si;
                reader.BaseStream.Position = offset.sl;
                return reader.ReadBytes(size.si);
            }
        }

        /// <summary>
        /// Formats data according to element rules
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="data">Data to format</param>
        /// <returns>Formatted string</returns>
        /// <exception cref="OutOfDataException">Raised if data from offset does not have enough bytes to make a
        /// number with width bytes</exception>
        private Bender.FormattedField FormatBuffer(Element el, byte[] data)
        {
            if (el.Elide)
            {
                return new Bender.FormattedField
                {
                    Name = el.Name,
                    Value = new List<string>
                    { string.Format("Elided {0} bytes", data.Length) }
                };
            }
        
            var value = new List<string>();

            if (!string.IsNullOrEmpty(el.Matrix))
            {
                value.AddRange(FormatMatrix(el, data));
            }
            else
            {
                try
                {
                    var number = Number.From(el, data);

                    switch (el.Format)
                    {
                        case ElementFormat.Binary:
                            // Make sure every byte has 8 places, 0 filled if needed
                            var binary = Convert.ToString(number.sl, 2).PadLeft(el.Units * 8, '0');
                            value.Add(string.Format("b{0}", binary));
                            break;
                        case ElementFormat.Octal:
                            value.Add(string.Format("O{0}", Convert.ToString(number.sl, 8)));
                            break;
                        case ElementFormat.Decimal:
                            value.Add(number.sl.ToString());
                            break;
                        case ElementFormat.Hex:
                            value.Add(string.Format("0x{0:X}", number.sl));
                            break;
                        case ElementFormat.ASCII:
                            value.Add(Encoding.ASCII.GetString(data));
                            break;
                        case ElementFormat.UTF16:
                            value.Add(Encoding.Unicode.GetString(data));
                            break;
                    }
                }
                catch (ArgumentException)
                {
                    throw new OutOfDataException(
                        "Element {0}: Not enough data left to create a {1} byte number ({2} bytes left)",
                        el.Name, el.Units, data.Length);
                }
            }

            return new Bender.FormattedField
            {
                Name = el.Name,
                Value = value
            };
        }

        /// <summary>
        /// Formats the nested payload type as a matrix
        /// </summary>
        /// <param name="el">Element descriptor</param>
        /// <param name="data">Raw data</param>
        /// <returns>List of formatted matrix rows</returns>
        private IEnumerable<string> FormatMatrix(Element el, byte[] data)
        {            
            if (_mSpec.Matrices == null)
            {
                return new List<string> { string.Format("No matrices specified but element {0} has referenced one", el.Name)};
            }

            var payload = _mSpec.Matrices.FirstOrDefault(p => el.Matrix.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if(payload == null)
            {
                return new List<string> { string.Format("Unknown matrix type {0} on element {1}", el.Matrix, el.Name) };
            }

            // Make a copy of Element and erase the payload name so we don't get stuck in a recursive loop
            var elClone = el.Clone();
            elClone.Matrix = string.Empty;
            elClone.Units = payload.Units;

            string Formatter(Element e, byte[] d)
            {
                return FormatBuffer(e, d).Value.First();
            }

            return payload.Format(elClone, data, Formatter);
        }
       
    }
}
