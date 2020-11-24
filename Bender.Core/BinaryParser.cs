namespace Bender.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Layouts;
    using Logging;
    using Nodes;

    /// <summary>
    /// Reads a binary file and formats the data as specified in the SpecFile
    /// </summary>
    public class BinaryParser : IDisposable
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        private static readonly ILog ReaderLog = LogProvider.GetLogger("ReaderLog");

        private readonly SpecFile _spec;
        private ReaderContext _reader;

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
            Ensure.IsNotNull(nameof(spec), spec);
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
            Ensure.IsNotNull(nameof(binary), binary);
            Ensure.IsValid(nameof(binary), !binary.Empty);

            var rootElement = new Element {Name = "root"};
            var bender = new Bender(new BPrimitive<Phrase>(rootElement, new Phrase(_spec.Name)));

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
                var errorTree = bender.Tree.AddChild(error);

                var next = ex.InnerException;
                while (!(next is null))
                {
                    error = new BError(next.Message, next.StackTrace ?? null, next);
                    errorTree.AddChild(error);

                    Log.Error(next, "Parser error");

                    next = ex.InnerException;
                }
            }

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
            var binaryReader = new BinaryReader(stream);
            _reader = new ReaderContext(binaryReader);

            ReaderLog.Debug("New reader created. Total size == {0}", _reader.Length);

            // Visit all elements an ensure their nested types are built
            foreach (var el in _spec.Elements)
            {
                LocateComplexTypes(el);
            }

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
        private void HandleSection(GetNextSection fnGetSection, ParseTree<BNode> tree)
        {
            var section = fnGetSection.Invoke();
            if (string.IsNullOrEmpty(section))
            {
                return;
            }

            Log.Debug("Handling '{0}'", section);

            // Find definition of the element
            var el = _spec.Elements.FirstOrDefault(o => o.Name.Equals(section));
            if (el is null)
            {
                Log.Warn("Section '{0}' is undefined", section);

                var error = new BError(section, "Undefined object");

                tree.AddChild(error);
            }
            else
            {
                if (el.IsArrayCount)
                {
                    var buff = ReadElementData(el);
                    var count = new Number(el, buff);
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
                    HandleElement(el, tree);
                }
            }
        }

        /// <summary>
        /// Parse element into node and append to tree
        /// </summary>
        /// <param name="el">Data definition</param>
        /// <param name="tree">Receives parsed nodes</param>
        private void HandleElement(Element el, ParseTree<BNode> tree)
        {
            var buff = ReadElementData(el);

            if (el.IsDeferred && buff.Length == 0)
            {
                Log.Info("'{0}' was declared deferred but is defined as empty", el.Name);

                var node = new BPrimitive<Phrase>(el,  new Phrase("Empty"));

                tree.AddChild(node);
            }
            else
            {
                el.BuildNode(_reader, tree, buff);
            }
        }

        /// <summary>
        /// Reads next element from current reader position
        /// </summary>
        /// <param name="el">Definition of what to read</param>
        /// <returns>Raw data the spec file says belongs to this element</returns>
        private byte[] ReadElementData(Element el)
        {
            byte[] buff;

            if (el.IsDeferred)
            {
                buff = _reader.DeferredRead();

                if (buff.Length == 0)
                {
                    ReaderLog.Debug("{0} is empty", el.Name);
                }
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
            ReaderLog.Info("{0,4}@0x{1:X4}/0x{2:X4} ({3})", count, _reader.Position, _reader.Length,
                section);

            return _reader.ReadBytes(count);
        }

        /// <summary>
        /// Return the request structure insance
        /// </summary>
        /// <param name="name">Structure name</param>
        /// <returns>Structure or null if no match is found</returns>
        private Structure GetStructure(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var result = _spec.Structures.FirstOrDefault(p =>
                name.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (result is null)
            {
                Log.Warn("'{0}' is not a defined structure");
            }

            return result;
        }

        /// <summary>
        /// Return the requested enumeration instance
        /// </summary>
        /// <param name="name">Enumeration name</param>
        /// <returns>Enumeration or null if no match is found</returns>
        private Enumeration GetEnumeration(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var result = _spec.Enumerations.FirstOrDefault(p =>
                name.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (result is null)
            {
                Log.Warn("'{0}' is not a defined enumeration");
            }

            return result;
        }

        /// <summary>
        ///     Recursively locate and instantiate all nested types
        ///     in this element
        /// </summary>
        /// <param name="el">Starting element</param>
        private void LocateComplexTypes(Element el)
        {
            // Load complex types before building the new node
            if (!string.IsNullOrEmpty(el.EnumerationName))
            {
                el.Enumeration = GetEnumeration(el.EnumerationName);
            }
            else if (!(string.IsNullOrEmpty(el.StructureName)))
            {
                el.Structure = GetStructure(el.StructureName);

                if (el.Structure is null)
                {
                    return;
                }

                foreach (var field in el.Structure.Elements)
                {
                    LocateComplexTypes(field);
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}