namespace Bender.Core
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
        private readonly SpecFile _spec;

        /// <summary>
        /// Creates a new parser
        /// </summary>
        /// <param name="spec">Binary layout spec</param>
        public BinaryParser(SpecFile spec)
        {
            _spec = spec;
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

        /// <summary>
        /// Parse binary file into a Bender file using the
        /// current SpecFile.
        /// </summary>
        /// <param name="binary">Source file</param>
        /// <returns>Parsed Bender</returns>
        private Bender TryParse(DataFile binary)
        {
            var bender = new Bender();

            using var stream = new MemoryStream(binary.Data);
            using var reader = new BinaryReader(stream);
            
            // Iterates over the order specified in 'layout'
            foreach (var section in _spec.Layout)
            {
                var els = new List<Element>();
                
                // If this is a structure, decompose its element now
                var st = _spec.Structures.FirstOrDefault(o => o.Name.Equals(section));
                if (st != null)
                {
                    els.AddRange(st.Elements);
                }
                else
                {
                    var el = _spec.Elements.FirstOrDefault(o => o.Name.Equals(section));
                    if (el != null)
                    {
                        els.Add(el);
                    }
                }

                // Neither a structure nor an element
                if (els.Count == 0)
                {
                    var formatted = new Bender.FormattedField
                    {
                        Name = section,
                        Value = new List<string> { "Undefined object" }
                    };
                    bender.FormattedFields.Add(formatted);
                }
                else
                {
                    // Handle all discovered elements
                    foreach (var el in els)
                    {
                        var formatted = HandleElement(el, reader, binary);
                        bender.FormattedFields.Add(formatted);
                    }
                }
            }

            return bender;
        }

        /// <summary>
        /// Process the fields of this element to build a formatted field
        /// </summary>
        /// <param name="el">Data definition</param>
        /// <param name="reader">Reader state</param>
        /// <param name="binary">Data source</param>
        /// <returns>Formatted result from element</returns>
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
            if (_spec.Deferreds == null)
            {
                return null;
            }

            var def = _spec.Deferreds.FirstOrDefault(d => d.Name != null && d.Name.Equals(el.Deferred));
            if (def == null)
            {
                return null;
            }

            var size = Number.From(def.SizeUnits, false, 0, buff);
            var offset = Number.From(def.OffsetUnits, false, size.si, buff);

            using var stream = new MemoryStream(binary.Data);
            using var reader = new BinaryReader(stream);
            
            el.Units = size.si;
            reader.BaseStream.Position = offset.sl;
            
            return reader.ReadBytes(size.si);
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
                        {
                            $"Elided {data.Length} bytes"
                        }
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
                            value.Add($"b{binary}");
                            break;
                        case ElementFormat.Octal:
                            value.Add($"O{Convert.ToString(number.sl, 8)}");
                            break;
                        case ElementFormat.Decimal:
                            value.Add(number.sl.ToString());
                            break;
                        case ElementFormat.Hex:
                        case ElementFormat.HexString:
                            var prefix = el.Format == ElementFormat.Hex ? "0x" : "";
                            var width = (el.Units * 2).NextPowerOf2();
                            var hex = Convert.ToString(number.sl, 16).PadLeft(width, '0');
                            value.Add($"{prefix}{hex}");
                            break;
                        case ElementFormat.ASCII:
                            value.Add(Encoding.ASCII.GetString(data));
                            break;
                        case ElementFormat.UTF16:
                            value.Add(Encoding.Unicode.GetString(data));
                            break;
                        
                        default:
                            value.Add($"Unsupported format: {el.Format}");
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
            if (_spec.Matrices == null)
            {
                return new List<string> {$"No matrices specified but element {el.Name} has referenced one"};
            }

            var payload = _spec.Matrices.FirstOrDefault(p => el.Matrix.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if(payload == null)
            {
                return new List<string> {$"Unknown matrix type {el.Matrix} on element {el.Name}"};
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
