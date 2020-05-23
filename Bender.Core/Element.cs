namespace Bender.Core
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Supported element formats
    /// </summary>
    public enum ElementFormat
    {
        /// <summary>
        /// Format value as binary with 'b' prefix and
        /// 8 characters per byte (left zero padded). LSB
        /// is on the right end of the string.
        /// e.g. b011100101
        /// </summary>
        Binary,
        /// <summary>
        /// Format value as octal with 'o' prefix
        /// </summary>
        Octal,
        /// <summary>
        /// Format as decimal
        /// </summary>
        Decimal,
        /// <summary>
        /// Format value as hex with '0x' prefix
        /// </summary>
        Hex,
        /// <summary>
        /// Parse as ASCII text
        /// </summary>
        ASCII,
        /// <summary>
        /// Format value as a UTF-16 string
        /// </summary>
        UTF16,
    }

    /// <summary>
    /// Element defines rules for a sequence of bytes. The name
    /// of an element must be unique to the specification file
    /// </summary>
    [DebuggerDisplay("Name = {Name}, Units = {Units}")]
    public class Element
    {
        /// <summary>
        /// Human friendly name of this element
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// True if bytes are in little Endian order
        /// </summary>
        [YamlMember(Alias = "little_endian", ApplyNamingConventions = false)]
        public bool LittleEndian { get; set; }

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
        /// How the bytes should be assembled
        /// </summary>
        public ElementFormat Format { get; set; }

        /// <summary>
        /// Number of bytes in element
        /// </summary>
        public int Units { get; set; }
        
        /// <summary>
        /// If this block contains a payload, Matrix value should match a known
        /// Matrix definition in the spec file
        /// </summary>
        public string Matrix { get; set; }    

        /// <summary>
        /// If this block is referencing future data, the deferred
        /// field must match an object in the deferreds list.
        /// </summary>
        public string Deferred { get; set; }
        
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

        /// <summary>
        ///     Returns all properties as newline delimited string
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Name: {0}\n", Name);
            sb.AppendFormat("Elide: {0}\n", Elide);
            sb.AppendFormat("Signed: {0}\n", IsSigned);
            sb.AppendFormat("Format: {0}\n", Format);
            sb.AppendFormat("Units: {0}\n", Units);
            sb.AppendFormat("Payload: {0}\n", Matrix);
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
                Format = Format,
                Units = Units,
                Matrix = Matrix,
            };
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Element element &&
                   Name == element.Name &&
                   LittleEndian == element.LittleEndian &&
                   IsSigned == element.IsSigned &&
                   Elide == element.Elide &&
                   Format == element.Format &&
                   Units == element.Units &&
                   Matrix == element.Matrix;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = 170416633;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
