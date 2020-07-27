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

        // Limits the recursion depth
        private const int NestedStructLimit = 64;
        private int _nestedDepth;

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
            var bender = new Bender();

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
                TryParse(bender, binary);
            }
            catch (Exception ex)
            {
                // Generated a neatly formatted field explaining the exceptions
                // and stacktrace leading to this problem.
                var value = $"{new string('*', ex.Message.Length)} {ex.Message} {ex.StackTrace}";
                var error = new BError("Parse Failure", value, ex);
                bender.Tree.Begin(error);

                var next = ex.InnerException;
                while (!(next is null))
                {
                    error = new BError(next.Message, next.StackTrace, next);
                    bender.Tree.Add(error);

                    Log.Error(next, "Parser error");

                    next = ex.InnerException;
                }
            }

            bender.Tree.End();

            return bender;
        }

        /// <summary>
        /// Parse binary file into a Bender file using the
        /// current SpecFile.
        /// </summary>
        /// <param name="bender">Bender target</param>
        /// <param name="binary">Source file</param>
        /// <returns>Parsed Bender</returns>
        /// <exception cref="ParseException">Raised if data from offset does not have enough bytes to make a
        /// number with width bytes</exception> 
        private void TryParse(Bender bender, DataFile binary)
        {
            using var stream = new MemoryStream(binary.Data);
            _reader = new BinaryReader(stream);
            _binary = binary;

            ReaderLog.Debug("New reader created. Total size == {0}", _reader.BaseStream.Length);

            // Iterates over the order specified in 'layout'
            var layoutQ = new Queue<string>(_spec.Layout);

            // Reads sections in order from spec file
            string SectionGetter()
            {
                return layoutQ.TryDequeue(out var result) ? result : string.Empty;
            }

            // Recursive calls may alter layoutQ so actively check the count
            while (layoutQ.Count > 0)
            {
                HandleSection(SectionGetter, bender.Tree);
            }
        }

        /// <summary>
        /// Process this section definition
        /// </summary>
        /// <param name="fnGetSection">Section name getter</param>
        /// <param name="tree">Receives Bender nodes</param>
        /// <returns>Formatted results from element</returns>
        /// <exception cref="ParseException">Raised if data from offset does not have enough bytes to make a
        /// number with width bytes</exception>
        private void HandleSection(GetNextSection fnGetSection, ParseTree tree)
        {
            var section = fnGetSection.Invoke();
            if (string.IsNullOrEmpty(section))
            {
                return;
            }

            Log.Debug("Handling '{0}'", section);

            // Find definition of the element
            var element = _spec.Elements.FirstOrDefault(o => o.Name.Equals(section));
            if (element is null)
            {
                Log.Warn("Section '{0}' is undefined", section);

                tree.Add(new BPrimitive(section, "Undefined object"));
            }
            else if (element.IsArrayCount)
            {
                var buff = ReadNextElement(element);
                var count = new Number(element, buff);
                var repeatedSection = fnGetSection();

                Log.Debug("'{0}' is an array with {1} elements", repeatedSection, count);

                for (var i = 0; i < count; ++i)
                {
                    // Repeats the same section without requiring it to actually be defined in the layout
                    HandleSection(() => repeatedSection, tree);
                }
            }
            else
            {
                HandleElement(element, tree);
            }
        }

        /// <summary>
        /// Process the fields of this element to build a formatted field
        /// </summary>
        /// <param name="el">Data definition</param>
        /// <param name="tree">Receives parsed nodes</param>
        /// <returns>Formatted result from element</returns>
        /// <exception cref="ParseException">Raised if data from offset does not have enough bytes to make a
        /// number with width bytes</exception>
        private void HandleElement(Element el, ParseTree tree)
        {
            var buff = ReadNextElement(el);
            if (buff is null)
            {
                Log.Warn("'{0}' is an invalid deferred object", el.Name);

                tree.Add(new BError(el.Name, "Error: Invalid deferred object"));
            }

            // This is declared as a deferral but the definition was marked as empty
            else if (el.IsDeferred && buff.Length == 0)
            {
                Log.Info("'{0}' was declared deferred but is defined as empty", el.Name);

                tree.Add(new BError(el.Name, "Empty"));
            }

            else
            {
                FormatBuffer(el, buff, tree);
            }
        }

        /// <summary>
        /// Formats data according to element rules
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="buff">Data to format</param>
        /// <param name="tree">Receives parsed nodes</param>
        /// <returns>Formatted string</returns>
        /// <exception cref="OutOfDataException">Raised if data from offset does not have enough bytes to make a
        /// number with width bytes</exception>
        private void FormatBuffer(Element el, byte[] buff, ParseTree tree)
        {
            if (el.Elide)
            {
                Log.Debug("'{0}' is elided", el.Name);

                tree.Add(new BPrimitive("Elided", $"{buff.Length} bytes"));
            }
            else
            {
                // Handle nestable types first
                if (!(el.Matrix is null))
                {
                    var formattedMatrix = FormatMatrix(el, buff);

                    tree.Add(formattedMatrix);
                }
                else if (!string.IsNullOrEmpty(el.Structure))
                {
                    var formattedStructure = FormatStructure(el, buff, tree);

                    tree.Add(formattedStructure);
                }
                else if (!string.IsNullOrEmpty(el.Enumeration))
                {
                    var formattedEnumeration = FormatEnumeration(el, buff);

                    tree.Add(formattedEnumeration);
                }
                else
                {
                    var formattedElement = el.TryFormat(buff);

                    tree.Add(formattedElement);
                }
            }
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
        private BNode FormatMatrix(Element el, byte[] buff)
        {
            Log.Debug("Formatting '{0}' as matrix '{1}'", el.Name, el.Matrix);

            // Make a copy of Element and erase the payload name so we don't get stuck in a recursive loop
            var elClone = el.Clone();
            elClone.Units = el.Clone().Matrix.Units;
            elClone.Matrix = null;

            if (elClone.Units == 0 || buff.Length == 0 || el.Matrix is null)
            {
                return null;
            }

            return MakeMatrix(buff, el.Matrix);
        }

        private BNode MakeMatrix(byte[] data, Matrix mat)
        {
            var cols = mat.Columns <= 0 ? 8 : mat.Columns;
            var rows = (data.Length / mat.Units) / cols;

            switch (mat.Units)
            {
                case 1:
                    return new BMatrix<byte>(data.Reshape(rows, cols));

                case 2:
                    var tShort = data.As<short>(false).Reshape(rows, cols);
                    return new BMatrix<short>(tShort);

                case 4:
                    var tInt = data.As<int>(false).Reshape(rows, cols);
                    return new BMatrix<int>(tInt);

                case 8:
                    var tLong = data.As<long>(false).Reshape(rows, cols);
                    return new BMatrix<long>(tLong);
            }

            return null;
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
        /// <param name="tree">Receives nested structure data</param>
        /// <returns>Structure formatted using this element's rules</returns>
        private BNode FormatStructure(Element el, byte[] buff, ParseTree tree)
        {
            Log.Debug("Formatting '{0}' as structure '{1}'", el.Name, el.Structure);

            if (_nestedDepth >= NestedStructLimit)
            {
                return new BError("Depth Exceeded", $"**Exceeded nested structure limit ({NestedStructLimit})**");
            }

            var def = GetStructure(el);
            if (def is null)
            {
                return new BError("Unknown Structure", $"Unknown structure type {el.Structure} on element {el.Name}");
            }

            Log.Debug("Using structure definition for '{0}'", def.Name);

            var structure = new BStructure(el.Name);
            tree.Begin(structure);

            // Make a copy of Element and erase the payload name so we don't get stuck in a recursive loop
            var elClone = el.Clone();
            elClone.Structure = string.Empty;

            // Buff is the binary representation of this structure definition
            // so reads must be relative to this buffer.
            using var stream = new MemoryStream(buff);
            using var innerReader = new BinaryReader(stream);

            // Temporarily set reader source to this structure's data
            var tempReader = _reader;
            _reader = innerReader;
            ReaderLog.Debug("Switching to temporary reader for {0} (Size == {1})", el.Name,
                _reader.BaseStream.Length);

            ++_nestedDepth;

            // If this is an implicit array we must track total bytes read
            var bytesRead = 0;
            do
            {
                // Recursively handle any nested elements
                foreach (var childEl in def.Elements)
                {
                    HandleElement(childEl, tree);

                    bytesRead += GetElementSize(childEl);
                }
            } while (el.IsArray && bytesRead < buff.Length);

            --_nestedDepth;

            // Restore the previous reader
            _reader = tempReader;
            ReaderLog.Debug("Continuing at offset {0}/{1}", _reader.BaseStream.Position,
                _reader.BaseStream.Length);

            tree.End();
            return structure;
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
        private BNode FormatEnumeration(Element el, byte[] buff)
        {
            Log.Debug("Formatting '{0}' as enumeration '{1}'", el.Name, el.Enumeration);

            var def = GetEnumeration(el);
            if (def is null)
            {
                return new BError("Unknown Enumeration",
                    "$Unknown enumeration type {el.Enumeration} on element {el.Name}");
            }

            Log.Debug("Using enumeration definition for '{0}'", def.Name);

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
                var size = new Number(sizeEl, buff);

                var offsetEl = new Element {Units = intWidth, Name = "offset_bytes"};
                buff = ReadNextElement(offsetEl);
                var offset = new Number(offsetEl, buff);

                if (size == 0 || offset == 0)
                {
                    ReaderLog.Debug("{0} is empty", el.Name);
                    return new byte[0];
                }

                ReaderLog.Debug("[deferred.abs]{0,4}@0x{1:X4}/0x{2:X4} ({3})", size.si, offset.sl,
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
        /// Reads count bytes from current reader
        /// </summary>
        /// <param name="count">Number of bytes to read</param>
        /// <param name="section">Name of section being read</param>
        /// <returns></returns>
        private byte[] ReadBytes(int count, string section)
        {
            ReaderLog.Info("{0,4}@0x{1:X4}/0x{2:X4} ({3})", count, _reader.BaseStream.Position,
                _reader.BaseStream.Length, section);

            return _reader.ReadBytes(count);
        }

        /// <summary>
        /// Locate structure by structure name on this element
        /// </summary>
        /// <param name="el">Element with structure definition</param>
        /// <returns>Structure or null if no match is found</returns>
        private Structure GetStructure(Element el)
        {
            if (el.Structure is null)
            {
                return null;
            }

            if (_spec.Structures == null)
            {
                Log.Warn("'{0}' references a structure but no structures are defined");
                return null;
            }

            var def = _spec.Structures.FirstOrDefault(p =>
                el.Structure.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (def == null)
            {
                Log.Warn("'{0}' references an undefined structure '{1}'", el.Name, el.Structure);
            }

            return def;
        }

        /// <summary>
        /// Locate enumeration by enumeration name on this element
        /// </summary>
        /// <param name="el">Element with enumeration definition</param>
        /// <returns>Enumeration of null if no match is found</returns>
        private Enumeration GetEnumeration(Element el)
        {
            if (el.Enumeration is null)
            {
                return null;
            }

            if (_spec.Enumerations == null)
            {
                Log.Warn("'{0}' references an enumeration but no enumerations are defined");
                return null;
            }

            var def = _spec.Enumerations.FirstOrDefault(p =>
                el.Enumeration.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (def == null)
            {
                Log.Warn("'{0}' references an undefined enumeration '{1}'", el.Name, el.Enumeration);
            }

            return def;
        }

        /// <summary>
        /// Returns the size in bytes of this element
        /// </summary>
        /// <param name="el">Element to measure</param>
        /// <returns>Size in bytes</returns>
        private int GetElementSize(Element el)
        {
            var childStruct = GetStructure(el);
            return childStruct?.Size ?? el.Size;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}