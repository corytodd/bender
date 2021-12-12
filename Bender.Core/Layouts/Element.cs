// ReSharper disable UnusedAutoPropertyAccessor.Global - This is a serialized type, all setters be global

namespace Bender.Core.Layouts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using JetBrains.Annotations;
    using Nodes;
    using Rendering;
    using YamlDotNet.Serialization;

    /// <summary>
    ///     Element defines rules for a sequence of bytes. The name
    ///     of an element must be unique to the specification file
    /// </summary>
    [DebuggerDisplay("Name = {Name}, Units = {Units}")]
    public class Element : ILayout
    {
        /// <summary>
        ///     Raw data this element is associated with
        /// </summary>
        private byte[] _rawData;

        /// <summary>
        ///     Human friendly name of this element
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     True if bytes are in little Endian order
        /// </summary>
        [YamlMember(Alias = "little_endian", ApplyNamingConventions = false)]
        public bool IsLittleEndian { get; set; }

        /// <summary>
        ///     True if value should be a signed type
        ///     Only applies to numbers.
        /// </summary>
        [YamlMember(Alias = "signed", ApplyNamingConventions = false)]
        public bool IsSigned { get; set; }

        /// <summary>
        ///     True to omit this element from printout
        /// </summary>
        public bool Elide { get; set; }

        /// <summary>
        ///     How the bytes should be interpreted for rendering
        /// </summary>
        [YamlMember(Alias = "format", ApplyNamingConventions = false)]
        public Bender.PrintFormat PrintFormat { get; set; }

        /// <summary>
        ///     Number of bytes in element
        /// </summary>
        public int Units { get; set; }

        /// <summary>
        ///     Matrix definition, if any
        /// </summary>
        public Matrix Matrix { get; set; }

        /// <summary>
        ///     True if this block is pointing to more data
        /// </summary>
        [YamlMember(Alias = "is_deferred", ApplyNamingConventions = false)]
        public bool IsDeferred { get; set; }

        /// <summary>
        ///     True if this value represents the count of an array. This
        ///     is considered an explicit array because it specifies that
        ///     N more of the next Element are to follow.
        /// </summary>
        [YamlMember(Alias = "is_array_count", ApplyNamingConventions = false)]
        public bool IsArrayCount { get; set; }

        /// <summary>
        ///     True if this value is an implicit array.
        ///     An implicit array means that the length is not encoded in the data
        ///     and that the outter container defines the size of each element. This
        ///     means that each element can be read implicitly by parsing until
        ///     the buffer is fully processed.
        /// </summary>
        [YamlMember(Alias = "is_array", ApplyNamingConventions = false)]
        public bool IsArray { get; set; }

        /// <summary>
        ///     Name of structure this element represents
        ///     In order to simplify the YAML file, structures
        ///     are forward declared and then referenced by name.
        /// </summary>
        [YamlMember(Alias = "structure", ApplyNamingConventions = false)]
        public string StructureName { get; set; }

        /// <summary>
        ///     If this block is referencing a structure, Structure
        ///     value should match a known structure definition
        /// </summary>
        [YamlIgnore]
        public Structure Structure { get; set; }

        /// <summary>
        ///     Name of enumeration this element represents
        ///     In order to simplify the YAML file, enumerations
        ///     are forward declared and then referenced by name.
        /// </summary>
        [YamlMember(Alias = "enumeration", ApplyNamingConventions = false)]
        public string EnumerationName { get; set; }

        /// <summary>
        ///     If this block's value should be interpreted as
        ///     an enumeration string, this name will map to
        ///     a predefined Enumeration element in the SpecFil.
        /// </summary>
        [YamlIgnore]
        public Enumeration Enumeration { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Size of this element will either be a constant 8 if a deferred
        ///     otherwise report the Units property.
        /// </summary>
        public int Size => IsDeferred ? 8 : Units;

        /// <inheritdoc />
        public IEnumerable<string> EnumerateLayout()
        {
            var content = ToTabbedString().Split('\n');
            foreach (var str in content)
            {
                yield return str;
            }
        }

        /// <summary>
        ///     Returns all properties as newline delimited string
        /// </summary>
        public string ToTabbedString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Name: {0}\n", Name);
            sb.AppendFormat("Elide: {0}\n", Elide);
            sb.AppendFormat("Signed: {0}\n", IsSigned);
            sb.AppendFormat("Format: {0}\n", PrintFormat);
            sb.AppendFormat("Units: {0}\n", Units);
            sb.AppendFormat("Payload: {0}\n", Matrix);
            sb.AppendFormat("Little Endian: {0}\n", IsLittleEndian);

            if (!(Structure is null))
            {
                sb.AppendFormat("Structure: {0}\n", Structure);
            }

            if (!(Enumeration is null))
            {
                sb.AppendFormat("Enumeration: {0}\n", Enumeration);
            }

            if (IsArray)
            {
                sb.AppendFormat("IsArray\n");
            }

            if (IsArrayCount)
            {
                sb.AppendFormat("IsArrayCount\n");
            }

            if (IsDeferred)
            {
                sb.AppendFormat("IsDeferred\n");
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Set raw data associated with this element
        /// </summary>
        /// <param name="context">Data provider context</param>
        /// <param name="tree">Tree context</param>
        /// <param name="data">Data this element should interpret</param>
        /// <param name="addChild">True to automatically add node to tree</param>
        public BNode BuildNode(ReaderContext context, ParseTree<BNode> tree, byte[] data, bool addChild)
        {
            Ensure.IsNotNull(nameof(data), data);

            _rawData = new byte[data.Length];
            Array.Copy(data, _rawData, _rawData.Length);

            BNode result = null;

            // Do not try to format the data if spec says to elide
            if (Elide)
            {
                result = new BPrimitive<Phrase>(this, new Phrase($"Elided {data.Length} bytes"));
            }
            else
            {
                try
                {
                    if (!(Enumeration is null))
                    {
                        result = BuildEnumeration();
                    }
                    else if (!(Matrix is null))
                    {
                        if (_rawData.Length == 0)
                        {
                            result = new BPrimitive<Phrase>(this, new Phrase("Empty matrix"));
                        }
                        else
                        {
                            result = BuildMatrix(context, tree);

                            // A Matrix may contain a structure which adds itself to the parse tree automatically
                            addChild = Structure is null;
                        }
                    }
                    else if (!(Structure is null))
                    {
                        result = BuildStructure(context, tree);

                        // Structure adds itself to the parse tree automatically
                        addChild = false;
                    }
                    else
                    {
                        IRenderable renderable = PrintFormat switch
                        {
                            Bender.PrintFormat.BigInt => new Phrase(string.Join("", data.Select(b => $"{b:X2}"))),
                            Bender.PrintFormat.Ascii => new Phrase(Encoding.ASCII.GetString(data)),
                            Bender.PrintFormat.Unicode => new Phrase(Encoding.Unicode.GetString(data)),
                            _ => new Number(this, data)
                        };

                        result = new BPrimitive<IRenderable>(this, renderable);
                    }
                }
                catch (Exception ex)
                {
                    result = new BError(this, "BuildNodeError", ex.Message, ex);
                }
            }

            if (addChild)
            {
                tree.AddChild(result);
            }

            return result;
        }

        /// <summary>
        ///     Deep copy this instance
        /// </summary>
        /// <returns>Copy of this</returns>
        public Element Clone()
        {
            var el = new Element
            {
                Name = Name,
                IsLittleEndian = IsLittleEndian,
                IsSigned = IsSigned,
                Elide = Elide,
                PrintFormat = PrintFormat,
                Units = Units,
                Matrix = Matrix,
                IsArray = IsArray,
                IsArrayCount = IsArrayCount,
                Enumeration = Enumeration,
                Structure = Structure,
                IsDeferred = IsDeferred
            };

            return el;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}, Units: {Units}, Format: {PrintFormat}, LE: {IsLittleEndian}, Elide: {Elide}";
        }

        /// <summary>
        ///     Parse and create Enumeration for this element
        /// </summary>
        /// <returns>Parsed node</returns>
        private BNode BuildEnumeration()
        {
            Ensure.IsNotNull(nameof(Enumeration), Enumeration);

            var number = new Number(this, _rawData);

            if (Enumeration.Values.TryGetValue(number.si, out var enumValue))
            {
                return new BPrimitive<Phrase>(this, new Phrase(enumValue));
            }
            else
            {
                return new BError(this, "Unknown Enumeration",
                    $"{number.si} is not defined in {Enumeration.Name}");
            }
        }

        /// <summary>
        ///     Create an empty matrix for the current PrintFormat
        /// </summary>
        /// <returns>Empty 2D array</returns>
        private IRenderable[,] BuildEmptyArray()
        {
            // If columns are not provided, infer it from
            // the length of raw data and matrix units
            var cols = Matrix.Columns == 0
                ? _rawData.Length / Matrix.Units
                : Matrix.Columns;

            var rows = (_rawData.Length / cols) / Matrix.Units;

            if (PrintFormat.IsString() || !(Structure is null))
            {
                return new Phrase[rows, cols];
            }
            else
            {
                return new Number[rows, cols];
            }
        }

        private BMatrix<IRenderable> FillMatrix<T>(IRenderable[,] data)
        {
            return new BMatrix<IRenderable>(this, data);
        }

        /// <summary>
        ///     Parse and create Matrix for this element
        /// </summary>
        /// <returns>Parsed node</returns>
        private BNode BuildMatrix(ReaderContext context, ParseTree<BNode> tree)
        {
            Ensure.IsNotNull(nameof(Matrix), Matrix);

            if (Matrix.Units == 0)
            {
                throw new ParseException("Matrix Units must be non-zero");
            }

            var matrix = BuildEmptyArray();
            var rows = matrix.GetLength(0);
            var cols = matrix.GetLength(1);

            var chunks = _rawData.AsChunks(Matrix.Units).ToArray();
            for (var row = 0; row < rows; ++row)
            {
                for (var col = 0; col < cols; ++col)
                {
                    var segment = chunks[row * cols + col];

                    if (!(Structure is null))
                    {
                        // Let structure append itself to the tree
                        BuildStructure(context, tree);

                        // We need a placeholder 
                        matrix[row, col] = new Phrase(string.Empty);
                    }
                    else if (PrintFormat.IsString())
                    {
                        matrix[row, col] = new Phrase(PrintFormat == Bender.PrintFormat.Ascii
                            ? Encoding.ASCII.GetString(segment)
                            : Encoding.Unicode.GetString(segment));
                    }
                    else
                    {
                        matrix[row, col] = new Number(Matrix.Units, IsSigned, IsLittleEndian, PrintFormat, segment);
                    }
                }
            }

            return new BMatrix<IRenderable>(this, matrix);
        }

        /// <summary>
        ///     Parse and create structure for this element
        /// </summary>
        /// <returns>Parsed node</returns>
        private BNode BuildStructure(ReaderContext context, ParseTree<BNode> tree)
        {
            Ensure.IsNotNull(nameof(Structure), Structure);
            Ensure.IsNotNull(nameof(context), context);

            var childReader = new ReaderContext(_rawData);

            var structure = new BStructure(this);

            var subTree = tree.AddChild(structure);

            foreach (var field in Structure.Elements)
            {
                byte[] fieldBytes;

                if (field.IsDeferred)
                {
                    var pointer = childReader.ReadBinaryPointer();

                    fieldBytes = context.DeferredRead(pointer);
                }
                else
                {
                    fieldBytes = childReader.ReadBytes(field.Units);
                }

                var node = field.BuildNode(context, subTree, fieldBytes, false);

                structure.Fields.Add(node);
            }

            return structure;
        }
    }
}