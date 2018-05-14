namespace BenderLib
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using YamlDotNet.Serialization;

    /// <summary>
    // Supported element formats
    /// </summary>
    public enum ElementFormat
    {
        Binary,
        Octal,
        Decimal,
        Hex,
        ASCII,
        UTF16,
        Float,
    }

    [DebuggerDisplay("Name = {Name}, Width = {Width}")]
    public class Element
    {
        /// <summary>
        /// Human friendly name of this element
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// True if btyes are in little Endian order
        /// </summary>
        [YamlMember(Alias = "little_endian", ApplyNamingConventions = false)]
        public bool LittleEndian { get; set; }

        /// <summary>
        /// True if value should be a signed type
        /// Only applies to numerics.
        /// </summary>
        [YamlMember(Alias = "signed", ApplyNamingConventions = false)]
        public bool IsSigned { get; set; }

        /// <summary>
        /// True to omit this element from printout
        /// </summary>
        public bool Elide { get; set; }

        /// <summary>
        /// True to disallow modiyfing this object
        /// </summary>
        [YamlMember(Alias = "readonly", ApplyNamingConventions = false)]
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// How the bytes should be assembled
        /// </summary>
        public ElementFormat Format { get; set; }

        /// <summary>
        /// Number of bytes in element
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// If this block contains a payload, Matrix value should match a known
        /// Matrix defintion in the spec file
        /// </summary>
        public string Matrix { get; set; }    
        
        /// <summary>
        /// Generator yields each line from ToString()
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> EnumerateLayout()
        {
            var content = ToString().Split('\n');
            foreach(var str in content)
            {
                yield return str;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Name: {0}\n", Name);
            sb.AppendFormat("Elide: {0}\n", Elide);
            sb.AppendFormat("Signed: {0}\n", IsSigned);
            sb.AppendFormat("Format: {0}\n", Format);
            sb.AppendFormat("Width: {0}\n", Width);
            sb.AppendFormat("Payload: {0}\n", Matrix);
            sb.AppendFormat("Read Only: {0}\n", IsReadOnly);
            sb.AppendFormat("Little Endian: {0}\n", LittleEndian);

            return sb.ToString();
        }

        /// <summary>
        /// Deep copy this instance
        /// </summary>
        /// <returns>Copy of this</returns>
        public Element Clone()
        {
            return new Element
            {
                Name = Name,
                LittleEndian = LittleEndian,
                IsSigned = IsSigned,
                Elide = Elide,
                IsReadOnly = IsReadOnly,
                Format = Format,
                Width = Width,
                Matrix = Matrix,
            };
        }

        public override bool Equals(object obj)
        {
            return obj is Element element &&
                   Name == element.Name &&
                   LittleEndian == element.LittleEndian &&
                   IsSigned == element.IsSigned &&
                   Elide == element.Elide &&
                   IsReadOnly == element.IsReadOnly &&
                   Format == element.Format &&
                   Width == element.Width &&
                   Matrix == element.Matrix;
        }

        public override int GetHashCode()
        {
            var hashCode = 170416633;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + LittleEndian.GetHashCode();
            hashCode = hashCode * -1521134295 + IsSigned.GetHashCode();
            hashCode = hashCode * -1521134295 + Elide.GetHashCode();
            hashCode = hashCode * -1521134295 + IsReadOnly.GetHashCode();
            hashCode = hashCode * -1521134295 + Format.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Matrix);
            return hashCode;
        }
    }
}
