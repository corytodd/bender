namespace Bender.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using YamlDotNet.Serialization;

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
        public Matrix Matrix { get; set; }

        /// <summary>
        /// True if this block is pointing to more data
        /// </summary>
        [YamlMember(Alias = "is_deferred", ApplyNamingConventions = false)]
        public bool IsDeferred { get; set; }

        /// <summary>
        /// True if this value represents the count of an array
        /// </summary>
        [YamlMember(Alias = "is_array_count", ApplyNamingConventions = false)]
        public bool IsArrayCount { get; set; }

        /// <summary>
        /// If this block is referencing a structure, Structure
        /// value should match a known structure definition
        /// </summary>
        public string Structure { get; set; }

        /// <summary>
        /// Generator yields each line from ToString()
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> EnumerateLayout()
        {
            var content = ToString().Split('\n');
            foreach (var str in content)
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
            sb.AppendFormat("Format: {0}\n", PrintFormat);
            sb.AppendFormat("Units: {0}\n", Units);
            sb.AppendFormat("Payload: {0}\n", Matrix);
            sb.AppendFormat("Little Endian: {0}\n", LittleEndian);

            return sb.ToString();
        }

        /// <summary>
        /// Formats data into an ordered list of matrix rows. Each row is
        /// formatted using the rules defined in element.
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="data">Data to format</param>
        /// <param name="elementFormatter">Converts extracted data into a formatted string</param>
        /// <returns>List of rows, formatted as strings</returns>
        public static IEnumerable<string> TryFormat(Element el, byte[] data, Bender.FormatElement elementFormatter)
        {
            var result = new List<string>();

            try
            {
                var number = Number.From(el, data);

                switch (el.PrintFormat)
                {
                    case Bender.PrintFormat.Binary:
                        // Make sure every byte has 8 places, 0 filled if needed
                        var binary = Convert.ToString(number.sl, 2).PadLeft(el.Units * 8, '0');
                        result.Add($"b{binary}");
                        break;
                    case Bender.PrintFormat.Octal:
                        result.Add($"O{Convert.ToString(number.sl, 8)}");
                        break;
                    case Bender.PrintFormat.Decimal:
                        result.Add(number.sl.ToString());
                        break;
                    case Bender.PrintFormat.Hex:
                    case Bender.PrintFormat.BigInt:
                        var prefix = el.PrintFormat == Bender.PrintFormat.Hex ? "0x" : "";
                        var width = (el.Units * 2).NextPowerOf2();
                        var hex = Convert.ToString(number.sl, 16).PadLeft(width, '0').ToUpper();
                        result.Add($"{prefix}{hex}");
                        break;
                    case Bender.PrintFormat.Ascii:
                        result.Add(Encoding.ASCII.GetString(data));
                        break;
                    case Bender.PrintFormat.Utf16:
                        result.Add(Encoding.Unicode.GetString(data));
                        break;
                    case Bender.PrintFormat.Float:
                        // Reinterpret data as floating point
                        var stream = new MemoryStream(data);
                        var reader = new BinaryReader(stream);
                        if (el.Units == 4 && data.Length == 4)
                        {
                            var f = reader.ReadSingle();
                            result.Add(f.ToString("F"));
                        }
                        else if (el.Units == 8 && data.Length == 8)
                        {
                            var d = reader.ReadDouble();
                            result.Add(d.ToString("F")); 
                        }
                        else
                        {
                            result.Add("Malformed float. Width must be 4 or 8 bytes");
                        }

                        break;

                    default:
                        result.Add($"Unsupported format: {el.PrintFormat}");
                        break;
                }
            }
            catch (ArgumentException)
            {
                throw new OutOfDataException(
                    "Element {0}: Not enough data left to create a {1} byte number ({2} bytes left)",
                    el.Name, el.Units, data.Length);
            }

            return result;
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
                PrintFormat = PrintFormat,
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
                   PrintFormat == element.PrintFormat &&
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