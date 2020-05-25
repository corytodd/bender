namespace Bender.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

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
                var element = _spec.Elements.FirstOrDefault(o => o.Name.Equals(section));
                if (element is null)
                {
                    var formatted = new Bender.FormattedField
                    {
                        Name = section,
                        Value = new List<string> {"Undefined object"}
                    };
                    bender.FormattedFields.Add(formatted);
                }
                else
                {
                    var formatted = HandleElement(element, reader, binary);
                    bender.FormattedFields.Add(formatted);
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
            if (el.IsDeferred)
            {
                buff = HandleDeferredRead(el, binary, buff);
                if (buff == null || buff.Length == 0)
                {
                    var message = buff is null ? "Error: Invalid deferred object" : "Empty";
                
                    return new Bender.FormattedField
                    {
                        Name = el.Name,
                        Value = new List<string> {message}
                    };
                }
            }

            return FormatBuffer(el, buff);
        }

        /// <summary>
        /// Use buff data to locate a deferred record. If the record can be located,
        /// it will be parsed according the element rules and the raw byte[] will be
        /// returned for further processing. If the object is empty, an empty
        /// buffer will be returned. If the deferred is not found or malformed,
        /// a null buffer will be returned.
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="binary">Binary source file</param>
        /// <param name="buff">Buffer record</param>
        /// <returns>Deferred data block</returns>
        private byte[] HandleDeferredRead(Element el, DataFile binary, byte[] buff)
        {
            const int intWidth = 4;    
            var size = Number.From(intWidth, false, 0, buff);
            var offset = Number.From(intWidth, false, intWidth, buff);

            if (size == 0 || offset == 0)
            {
                return new byte[0];
            }

            using var stream = new MemoryStream(binary.Data);
            using var reader = new BinaryReader(stream);

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
            else if (!string.IsNullOrEmpty(el.Structure))
            {
                value.AddRange(FormatStructure(el, data));
            }
            else
            {
                value.AddRange(el.TryFormat(el, data, DefaultFormatter));
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
                return new List<string> {$"No matrices specified but element {el.Name} has referenced {el.Matrix}"};
            }

            var payload = _spec.Matrices.FirstOrDefault(p =>
                el.Matrix.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (payload == null)
            {
                return new List<string> {$"Unknown matrix type {el.Matrix} on element {el.Name}"};
            }

            // Make a copy of Element and erase the payload name so we don't get stuck in a recursive loop
            var elClone = el.Clone();
            elClone.Matrix = string.Empty;
            elClone.Units = payload.Units;

            return payload.TryFormat(elClone, data, DefaultFormatter);
        }

        private IEnumerable<string> FormatStructure(Element el, byte[] data)
        {
            if (_spec.Structures == null)
            {
                return new List<string> {$"No structure specified but element {el.Name} has referenced {el.Structure}"};
            }

            var payload = _spec.Structures.FirstOrDefault(p =>
                el.Structure.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (payload == null)
            {
                return new List<string> {$"Unknown structure type {el.Structure} on element {el.Name}"};
            }

            // Make a copy of Element and erase the payload name so we don't get stuck in a recursive loop
            var elClone = el.Clone();
            elClone.Structure = string.Empty;

            return payload.TryFormat(elClone, data, DefaultFormatter);
        }

        private string DefaultFormatter(Element e, byte[] d)
        {
            return FormatBuffer(e, d).Value.First();
        }
    }
}