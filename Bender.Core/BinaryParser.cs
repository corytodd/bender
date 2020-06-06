namespace Bender.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Logging;

    /// <summary>
    /// Reads a binary file and formats the data as specified in the SpecFile
    /// </summary>
    public class BinaryParser : IDisposable
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        private static readonly ILog ReaderLog = LogProvider.GetLogger("ReaderLog");

        private readonly SpecFile _spec;
        private BinaryReader _reader;
        private DataFile _binary;

        // Limits the recursion deptch
        private const int NestedStructLimit = 64;
        private int _nestedDepth;

        // Keeps track of progress in the even of an exception
        private readonly Bender _shadowCopy;

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
            _shadowCopy = new Bender();
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
            catch (Exception ex)
            {
                var errorField = new Bender.FormattedField
                {
                    Name = $"Parse Failure",
                    Value = new List<string>
                    {
                        new string('*', ex.Message.Length),
                        ex.Message,
                        ex.StackTrace
                    }
                };

                var next = ex.InnerException;
                while (!(next is null))
                {
                    errorField.Value.Add(next.Message);
                    errorField.Value.Add(next.StackTrace);

                    Log.Error(next, "Parser error");

                    next = ex.InnerException;
                }

                _shadowCopy.FormattedFields.Add(errorField);

                return _shadowCopy;
            }
        }

        /// <summary>
        /// Parse binary file into a Bender file using the
        /// current SpecFile.
        /// </summary>
        /// <param name="binary">Source file</param>
        /// <returns>Parsed Bender</returns>
        /// <exception cref="ParseException">Raised if data from offset does not have enough bytes to make a
        /// number with width bytes</exception> 
        private Bender TryParse(DataFile binary)
        {
            var bender = new Bender();

            using var stream = new MemoryStream(binary.Data);
            _reader = new BinaryReader(stream);
            _binary = binary;

            ReaderLog.Debug("New reader created. Total size == {0}", _reader.BaseStream.Length);

            // Iterates over the order specified in 'layout'
            var queue = new Queue<string>(_spec.Layout);

            string SectionGetter()
            {
                return queue.TryDequeue(out var result) ? result : string.Empty;
            }

            while (queue.Count > 0)
            {
                var formatted = HandleSection(SectionGetter);

                foreach (var f in formatted)
                {
                    bender.FormattedFields.Add(f);

                    _shadowCopy.FormattedFields.Add(f);
                }
            }

            return bender;
        }

        /// <summary>
        /// Process this section definition
        /// </summary>
        /// <param name="fnGetSection">Section name getter</param>
        /// <returns>Formatted results from element</returns>
        /// <exception cref="ParseException">Raised if data from offset does not have enough bytes to make a
        /// number with width bytes</exception>
        private IEnumerable<Bender.FormattedField> HandleSection(GetNextSection fnGetSection)
        {
            var section = fnGetSection.Invoke();

            Log.Debug("Handling '{0}'", section);

            if (string.IsNullOrEmpty(section))
            {
                return Enumerable.Empty<Bender.FormattedField>();
            }

            var result = new List<Bender.FormattedField>();

            // Find definition of the element
            var element = _spec.Elements.FirstOrDefault(o => o.Name.Equals(section));
            if (element is null)
            {
                Log.Warn("Section '{0}' is undefined", section);

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

                Log.Debug("'{0}' is an array with {1} elements", repeatedSection, count);

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
        /// <exception cref="ParseException">Raised if data from offset does not have enough bytes to make a
        /// number with width bytes</exception>
        private Bender.FormattedField HandleElement(Element el)
        {
            var buff = ReadNextElement(el);
            if (buff is null)
            {
                Log.Warn("'{0}' is an invalid deferred object", el.Name);

                return new Bender.FormattedField
                {
                    Name = el.Name,
                    Value = new List<string> {"Error: Invalid deferred object"}
                };
            }

            // This is declared as a deferral but the definition was marked as empty
            if (el.IsDeferred && buff.Length == 0)
            {
                Log.Info("'{0}' was declared deferred but is defined as empty", el.Name);

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
            // An element may span multiple lines. Each element 
            // of 'value' is a line in the string formatting of this element.
            var value = new List<string>();


            if (el.Elide)
            {
                Log.Debug("'{0}' is elided", el.Name);

                value.Add($"Elided {buff.Length} bytes");
            }
            else
            {
                // Handle nestable types first
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
                else if (!string.IsNullOrEmpty(el.Enumeration))
                {
                    var formattedEnumeration = FormatEnumeration(el, buff);

                    value.Add(formattedEnumeration);
                }
                else
                {
                    var formattedElement = el.TryFormat(buff);

                    value.AddRange(formattedElement);
                }
            }

            return new Bender.FormattedField
            {
                Name = el.Name,
                Value = value
            };
        }

        /// <summary>
        /// Formats the nested payload type as a matrix.
        /// 
        /// <remarks>If the matrix is not defined, a string describing
        /// error will instead be returned.
        /// </remarks>
        /// 
        /// </summary>
        /// <param name="el">Element descriptor</param>
        /// <param name="buff">Raw data</param>
        /// <returns>List of formatted matrix rows</returns>
        /// <exception cref="ParseException">Raised if data from offset does not have enough bytes to make a
        /// number with width bytes</exception>
        private IEnumerable<string> FormatMatrix(Element el, byte[] buff)
        {
            Log.Debug("Formatting '{0}' as matrix '{1}'", el.Name, el.Matrix);

            // Make a copy of Element and erase the payload name so we don't get stuck in a recursive loop
            var elClone = el.Clone();
            elClone.Units = el.Clone().Matrix.Units;
            elClone.Matrix = null;

            return el.Matrix.TryFormat(elClone, buff, DefaultFormatter);
        }

        /// <summary>
        /// Formats the buffer as a structure. If the structure is
        /// not defined, a string describing the error will be returned.
        /// If there is an parser issue such as missing or malformed
        /// data, an exception will be thrown.
        /// 
        /// <remarks>
        /// If the structure is not defined, a string describing
        /// error will instead be returned.
        /// </remarks>
        /// 
        /// <remarks>
        /// This supports up to two nested structures. After two levels of nesting, the parser will break and
        /// we do not have a mechanism to test this yet.
        /// </remarks>
        /// </summary>
        /// <param name="el">Element definition</param>
        /// <param name="buff">Raw data</param>
        /// <returns>Structure formatted using this element's rules</returns>
        private IEnumerable<string> FormatStructure(Element el, byte[] buff)
        {
            Log.Debug("Formatting '{0}' as structure '{1}'", el.Name, el.Structure);

            if (_nestedDepth >= NestedStructLimit)
            {
                return new[] {$"**Exceeded nested structure limit ({NestedStructLimit})**"};
            }

            if (_spec.Structures == null)
            {
                Log.Warn("'{0}' references a structure but no structures are defined");

                return new List<string> {$"No structure specified but element {el.Name} has referenced {el.Structure}"};
            }

            var def = _spec.Structures.FirstOrDefault(p =>
                el.Structure.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (def == null)
            {
                Log.Warn("'{0}' references an undefined structure '{1}'", el.Name, el.Structure);

                return new List<string> {$"Unknown structure type {el.Structure} on element {el.Name}"};
            }

            Log.Debug("Using structure definition for '{0}'", def.Name);

            // Make a copy of Element and erase the payload name so we don't get stuck in a recursive loop
            var elClone = el.Clone();
            elClone.Structure = string.Empty;

            var result = new List<string>(def.Elements.Count);

            using var stream = new MemoryStream(buff);
            using var innerReader = new BinaryReader(stream);

            // Temporary set reader source to this structure's data
            var tempReader = _reader;
            _reader = innerReader;

            ReaderLog.Debug("Switching to temporary reader for {0} (Size == {1})", el.Name,
                _reader.BaseStream.Length);
            
            ++_nestedDepth;
            
            if (el.IsDeferred)
            {
                
                // Recursively handle any nested elements, structures included
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
                
            }
            else
            {
                foreach (var childEl in def.Elements)
                {
                    var field = ReadNextElement(childEl);
                    result.Add($"[ {childEl.Name}: {DefaultFormatter(childEl, field)} ]");
                }
            }
            
            --_nestedDepth;
            
            // Restore the previous reader
            _reader = tempReader;
            ReaderLog.Debug("Continuing at offset {0}/{1}", _reader.BaseStream.Position,
                _reader.BaseStream.Length);

            return result;
        }

        /// <summary>
        /// Formats the buffer as an enumeration.
        ///
        /// <remarks>
        /// If the matrix is not defined, a string describing
        /// error will instead be returned.
        /// </remarks>
        /// 
        /// </summary>
        /// <param name="el">Element definition</param>
        /// <param name="buff">Raw data</param>
        /// <returns>Enumeration string for the value in buff</returns>
        private string FormatEnumeration(Element el, byte[] buff)
        {
            Log.Debug("Formatting '{0}' as enumeration '{1}'", el.Name, el.Enumeration);

            if (_spec.Enumerations == null)
            {
                Log.Warn("'{0}' references an enumeration but no enumerations are defined");

                return $"No enumerations specified but element {el.Name} has referenced {el.Enumeration}";
            }

            var def = _spec.Enumerations.FirstOrDefault(p =>
                el.Enumeration.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (def == null)
            {
                Log.Warn("'{0}' references an undefined enumeration '{1}'", el.Name, el.Enumeration);

                return $"Unknown enumeration type {el.Enumeration} on element {el.Name}";
            }

            return el.TryFormatEnumeration(def, buff);
        }

        /// <summary>
        /// Reads next element from current reader position
        /// </summary>
        /// <param name="el">Definition of what to read</param>
        /// <returns>Raw data the spec file says belongs to this element</returns>
        private byte[] ReadNextElement(Element el)
        {
            byte[] buff;

            if (el.IsDeferred)
            {
                // Deferred object is always 8 bytes (2 ints)
                const int intWidth = 4;

                var sizeEl = new Element {Units = intWidth, Name = "size_bytes"};
                buff = ReadNextElement(sizeEl);
                var size = Number.From(sizeEl, buff);

                var offsetEl = new Element {Units = intWidth, Name = "offset_bytes"};
                buff = ReadNextElement(offsetEl);
                var offset = Number.From(offsetEl, buff);

                if (size == 0 || offset == 0)
                {
                    ReaderLog.Debug("{0} is empty", el.Name);
                    return new byte[0];
                }

                ReaderLog.Debug("[deferred.abs]0x{0:X4}b@0x{1:X4}/0x{2:X4} ({3})", size.si, offset.sl,
                    _binary.Data.Length, el.Name);

                // Create a new reader so we don't interfere with the current element
                using var stream = new MemoryStream(_binary.Data);
                using var reader = new BinaryReader(stream);

                // Read the deferred object in its entirety
                reader.BaseStream.Position = offset.sl;
                buff = reader.ReadBytes(size.si);
            }
            else
            {
                buff = ReadBytes(el.Units, el.Name);

                // If byte order does not match, flip now
                if (!(el.IsLittleEndian && BitConverter.IsLittleEndian))
                {
                    Array.Reverse(buff);
                }
            }

            return buff;
        }

        /// <summary>
        /// Applies internal formatting rules to render element data
        /// </summary>
        private Bender.FormattedField DefaultFormatter(Element el, byte[] buff)
        {
            var formatted = FormatBuffer(el, buff);
            return formatted;
        }

        /// <summary>
        /// Reads count bytes from current reader
        /// </summary>
        /// <param name="count">Number of bytes to read</param>
        /// <param name="section">Name of section being read</param>
        /// <returns></returns>
        private byte[] ReadBytes(int count, string section)
        {
            ReaderLog.Info("0x{0:X4}b@0x{1:X4}/0x{2:X4} ({3})", count, _reader.BaseStream.Position,
                _reader.BaseStream.Length, section);

            return _reader.ReadBytes(count);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}