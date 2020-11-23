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
    /// Element defines rules for a sequence of bytes. The name
    /// of an element must be unique to the specification file
    /// </summary>
    [DebuggerDisplay("Name = {Name}, Units = {Units}")]
    public class Element : ILayout
    {
        /// <summary>
        /// Do not build a node more than once
        /// </summary>
        private bool _debugIsBuilt;

        /// <summary>
        /// Raw data this element is associated with
        /// </summary>
        private byte[] _rawData;

        /// <summary>
        /// Parsed form of <see cref="_rawData"/>
        /// </summary>
        private List<IRenderable> _payload = new();

        /// <summary>
        /// Human friendly name of this element
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// True if bytes are in little Endian order
        /// </summary>
        [YamlMember(Alias = "little_endian", ApplyNamingConventions = false)]
        public bool IsLittleEndian { get; set; }

        /// <summary>
        /// True if value should be a signed type
        /// Only applies to numbers.
        /// </summary>
        [YamlMember(Alias = "signed", ApplyNamingConventions = false)]
        public bool IsSigned { get; set; }

        /// <summary>
        /// True to omit this element from printout
        /// </summary>
        public bool Elide { get; set; }

        /// <summary>
        /// How the bytes should be interpreted for rendering
        /// </summary>
        [YamlMember(Alias = "format", ApplyNamingConventions = false)]
        public Bender.PrintFormat PrintFormat { get; set; }

        /// <summary>
        /// Number of bytes in element
        /// </summary>
        public int Units { get; set; }

        /// <summary>
        /// Matrix definition, if any
        /// </summary>
        [CanBeNull]
        public Matrix Matrix { get; set; }

        /// <summary>
        /// True if this block is pointing to more data
        /// </summary>
        [YamlMember(Alias = "is_deferred", ApplyNamingConventions = false)]
        public bool IsDeferred { get; set; }

        /// <summary>
        /// True if this value represents the count of an array. This
        /// is considered an explicit array because it specifies that
        /// N more of the next Element are to follow.
        /// </summary>
        [YamlMember(Alias = "is_array_count", ApplyNamingConventions = false)]
        public bool IsArrayCount { get; set; }

        /// <summary>
        /// True if this value is an implicit array.
        /// An implicit array means that the length is not encoded in the data
        /// and that the outter container defines the size of each element. This
        /// means that each element can be read implicitly by parsing until
        /// the buffer is fully processed.
        /// </summary>
        [YamlMember(Alias = "is_array", ApplyNamingConventions = false)]
        public bool IsArray { get; set; }

        /// <summary>
        /// Name of structure this element represents
        /// In order to simplify the YAML file, structures
        /// are forward declared and then referenced by name.
        /// </summary>
        [YamlMember(Alias = "structure", ApplyNamingConventions = false)]
        public string StructureName { get; set; }

        /// <summary>
        /// If this block is referencing a structure, Structure
        /// value should match a known structure definition
        /// </summary>
        [YamlIgnore]
        [CanBeNull]
        public Structure Structure { get; set; }

        /// <summary>
        /// Name of enumeration this element represents
        /// In order to simplify the YAML file, enumerations
        /// are forward declared and then referenced by name.
        /// </summary>
        [YamlMember(Alias = "enumeration", ApplyNamingConventions = false)]
        public string EnumerationName { get; set; }

        /// <summary>
        /// If this block's value should be interpreted as
        /// an enumeration string, this name will map to
        /// a predefined Enumeration element in the SpecFil.
        /// </summary>
        [YamlIgnore]
        [CanBeNull]
        public Enumeration Enumeration { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Size of this element will either be a constant 8 if a deferred
        /// otherwise report the Units property.
        /// </summary>
        public int Size => IsDeferred ? 8 : Units;

        /// <summary>
        /// Parsedf data associated with this element
        /// </summary>
        public IEnumerable<IRenderable> Payload => _payload;

        /// <summary>
        /// Set raw data associated with this element
        /// </summary>
        /// <param name="data">Data this element should interpret</param>
        public BNode BuildNode(byte[] data)
        {
            Ensure.IsNotNull(nameof(data), data);
            //Debug.Assert(!_debugIsBuilt);

            _rawData = new byte[data.Length];
            Array.Copy(data, _rawData, _rawData.Length);

            BNode? result = null;

            // Do not try to format the data if spec says to elide
            if (Elide)
            {
                result = new BString(this, $"Elided {data.Length} bytes");
            }
            else
            {
                try
                {
                    if (!(Enumeration is null))
                    {
                        var number = new Number(this, data);

                        if (Enumeration.Values.TryGetValue(number.si, out var enumValue))
                        {
                            result = new BString(this, enumValue);
                        }
                        else
                        {
                            result = new BError("Unknown Enumeration",
                                $"{number.si} is not defined in {Enumeration.Name}");
                        }
                    }
                    else if (!(Matrix is null))
                    {
                        var rows = (Size / Matrix.Columns) / Matrix.Units;
                        var matrix = new Number[rows, Matrix.Columns];
                        var chunks = _rawData.AsChunks(Matrix.Units).ToList();
                        for (var row = 0; row < rows; ++row)
                        {
                            for (var col = 0; col < Matrix.Columns; ++col)
                            {
                                matrix[row, col] = new Number(Matrix.Units, IsSigned, IsLittleEndian, PrintFormat,
                                    chunks[row * Matrix.Columns + col]);
                            }
                        }

                        result = new BMatrix<Number>(this, matrix);
                    }
                    else if (!(Structure is null))
                    {
                        using var stream = new MemoryStream(data);
                        using var reader = new BinaryReader(stream);

                        var structure = new BStructure(this);
                        foreach (var child in Structure.Elements)
                        {
                            var node = child.BuildNode(reader.ReadBytes(child.Units));

                            structure.Fields.Add(node);
                        }

                        result = structure;
                    }
                    else
                    {
                        result = PrintFormat switch
                        {
                            Bender.PrintFormat.Ascii => new BString(this, Encoding.ASCII.GetString(data)),
                            Bender.PrintFormat.Unicode => new BString(this, Encoding.Unicode.GetString(data)),
                            _ => new BPrimitive<Number>(this, new Number(this, data))
                        };
                    }
                }
                catch (Exception ex)
                {
                    result = new BError($"{Name} BuildNodeError", ex.Message, ex);
                }
            }

            _payload = new List<IRenderable>
            {
                result
            };

            _debugIsBuilt = true;

            return result;
        }

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
        /// Deep copy this instance
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
    }
}