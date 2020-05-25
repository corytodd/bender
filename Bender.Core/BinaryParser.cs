namespace Bender.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Reads a binary file and formats the data as specified in the SpecFile
    /// </summary>
    public class BinaryParser : IDisposable

    {
        private readonly SpecFile _spec;
        private BinaryReader _reader;
        private DataFile _binary;

        /// <summary>
        /// Returns name of next section in layout
        /// </summary>
        private delegate string GetNextSection();

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
            _reader = new BinaryReader(stream);
            _binary = binary;

            // Iterates over the order specified in 'layout'
            var queue = new Queue<string>(_spec.Layout);

            string SectionGetter()
            {
                return queue.TryDequeue(out var result) ? result : string.Empty;
            }

            while (queue.Count > 0)
            {
                var formatted = HandleSection(SectionGetter);

                bender.FormattedFields.AddRange(formatted);
            }

            return bender;
        }

        /// <summary>
        /// Process this section definition
        /// </summary>
        /// <param name="fnGetSection">Section name getter</param>
        /// <returns>Formatted results from element</returns>
        private IEnumerable<Bender.FormattedField> HandleSection(GetNextSection fnGetSection)
        {
            var section = fnGetSection.Invoke();
            if (string.IsNullOrEmpty(section))
            {
                return Enumerable.Empty<Bender.FormattedField>();
            }

            var result = new List<Bender.FormattedField>();

            // Find definition of the element
            var element = _spec.Elements.FirstOrDefault(o => o.Name.Equals(section));
            if (element is null)
            {
                result.Add(new Bender.FormattedField
                {
                    Name = section,
                    Value = new List<string> {"Undefined object"}
                });
            }
            else if (element.IsArrayCount)
            {
                var buff = ReadNextElement(element);
                var count = Number.From(element, buff);
                var repeatedSection = fnGetSection();

                for (var i = 0; i < count; ++i)
                {
                    // Repeats the same section without requiring it to actually be defined in the layout
                    var formattedSection = HandleSection(() => repeatedSection);

                    result.AddRange(formattedSection);
                }
            }
            else
            {
                var formattedElement = HandleElement(element);

                result.Add(formattedElement);
            }

            return result;
        }

        /// <summary>
        /// Process the fields of this element to build a formatted field
        /// </summary>
        /// <param name="el">Data definition</param>
        /// <returns>Formatted result from element</returns>
        private Bender.FormattedField HandleElement(Element el)
        {
            var buff = ReadNextElement(el);
            if (buff is null)
            {
                return new Bender.FormattedField
                {
                    Name = el.Name,
                    Value = new List<string> {"Error: Invalid deferred object"}
                };
            }

            // This is declared as a deferral but the definition was marked as empty
            if (el.IsDeferred && buff.Length == 0)
            {
                return new Bender.FormattedField
                {
                    Name = el.Name,
                    Value = new List<string> {"Empty"}
                };
            }

            return FormatBuffer(el, buff);
        }

        /// <summary>
        /// Formats data according to element rules
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="buff">Data to format</param>
        /// <returns>Formatted string</returns>
        /// <exception cref="OutOfDataException">Raised if data from offset does not have enough bytes to make a
        /// number with width bytes</exception>
        private Bender.FormattedField FormatBuffer(Element el, byte[] buff)
        {
            if (el.Elide)
            {
                return new Bender.FormattedField
                {
                    Name = el.Name,
                    Value = new List<string>
                    {
                        $"Elided {buff.Length} bytes"
                    }
                };
            }

            var value = new List<string>();

            if (!(el.Matrix is null))
            {
                var formattedMatrix = FormatMatrix(el, buff);

                value.AddRange(formattedMatrix);
            }
            else if (!string.IsNullOrEmpty(el.Structure))
            {
                var formattedStructure = FormatStructure(el, buff);

                value.AddRange(formattedStructure);
            }
            else
            {
                var formattedElement = el.TryFormat(el, buff, DefaultFormatter);

                value.AddRange(formattedElement);
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
        /// <param name="buff">Raw data</param>
        /// <returns>List of formatted matrix rows</returns>
        private IEnumerable<string> FormatMatrix(Element el, byte[] buff)
        {
            // Make a copy of Element and erase the payload name so we don't get stuck in a recursive loop
            var elClone = el.Clone();
            elClone.Units = el.Clone().Matrix.Units;
            elClone.Matrix = null;

            return el.Matrix.TryFormat(elClone, buff, DefaultFormatter);
        }

        private IEnumerable<string> FormatStructure(Element el, byte[] buff)
        {
            if (_spec.Structures == null)
            {
                return new List<string> {$"No structure specified but element {el.Name} has referenced {el.Structure}"};
            }

            var def = _spec.Structures.FirstOrDefault(p =>
                el.Structure.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (def == null)
            {
                return new List<string> {$"Unknown structure type {el.Structure} on element {el.Name}"};
            }

            // Make a copy of Element and erase the payload name so we don't get stuck in a recursive loop
            var elClone = el.Clone();
            elClone.Structure = string.Empty;

            var result = new List<string>(def.Elements.Count);

            using var stream = new MemoryStream(buff);
            using var innerReader = new BinaryReader(stream);

            if (el.IsDeferred)
            {

                var tempReader = _reader;
                _reader = innerReader;
                
                foreach (var childEl in def.Elements)
                {
                    var formatted = HandleElement(childEl);
                    var isFirst = true;
                    foreach (var value in formatted.Value)
                    {
                        // Repeat name only once for each element, maintain padding
                        var prefix = isFirst ? childEl.Name : new string(' ', childEl.Name.Length);
                        result.Add($"[ {prefix}: {value} ]");
                        isFirst = false;
                    }
                }

                _reader = tempReader;
            }
            else
            {
                foreach (var childEl in def.Elements)
                {
                    var field = innerReader.ReadBytes(childEl.Units);
                    result.Add($"[ {childEl.Name}: {DefaultFormatter(childEl, field)} ]");
                }
            }

            return result;
        }

        private byte[] ReadNextElement(Element el)
        {
            byte[] buff;

            if (el.IsDeferred)
            {
                // Deferred object is always 8 bytes (2 ints)
                buff = _reader.ReadBytes(8);
                const int intWidth = 4;
                var size = Number.From(intWidth, false, 0, buff);
                var offset = Number.From(intWidth, false, intWidth, buff);

                if (size == 0 || offset == 0)
                {
                    return new byte[0];
                }

                // Create a new reader so we don't interfere with the current element
                using var stream = new MemoryStream(_binary.Data);
                using var reader = new BinaryReader(stream);

                // Read the deferred object in its entirety
                reader.BaseStream.Position = offset.sl;
                buff = reader.ReadBytes(size.si);
            }
            else
            {
                buff = _reader.ReadBytes(el.Units);
            }

            // If byte order does not match, flip now
            if (!(el.LittleEndian && BitConverter.IsLittleEndian))
            {
                Array.Reverse(buff);
            }

            return buff;
        }

        private string DefaultFormatter(Element e, byte[] d)
        {
            return FormatBuffer(e, d).Value.First();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}