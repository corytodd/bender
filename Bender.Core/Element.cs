// ReSharper disable UnusedAutoPropertyAccessor.Global - This is a serialized type, all setters be global
namespace Bender.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Element defines rules for a sequence of bytes. The name
    /// of an element must be unique to the specification file
    /// </summary>
    [DebuggerDisplay("Name = {Name}, Units = {Units}")]
    public class Element : ILayout
    {
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
        /// If this block is referencing a structure, Structure
        /// value should match a known structure definition
        /// </summary>
        public string Structure { get; set; }

        /// <summary>
        /// If this block's value should be interpreted as
        /// an enumeration string, this name will map to
        /// a predefined Enumeration element in the SpecFil.
        /// </summary>
        public string Enumeration { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Size of this element will either be a constant 8 if a deferred
        /// otherwise report the Units property.
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
            
            if (!string.IsNullOrEmpty(Structure))
            {
                sb.AppendFormat("Structure: {0}\n", Structure);
            }

            if (!string.IsNullOrEmpty(Enumeration))
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
        /// Formats data into an ordered list of matrix rows. Each row is
        /// formatted using the rules defined in element.
        /// </summary>
        /// <param name="data">Data to format</param>
        /// <returns>List of rows, formatted as strings</returns>
        public IEnumerable<string> TryFormat(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            
            var result = new List<string>();

            try
            {
                switch (PrintFormat)
                {
                    case Bender.PrintFormat.Ascii:
                        result.Add(Encoding.ASCII.GetString(data));
                        break;

                    case Bender.PrintFormat.Unicode:
                        result.Add(Encoding.Unicode.GetString(data));
                        break;

                    default:
                        var number = new Number(this, data);
                        var formatted = FormatNumber(number);
                        result.Add(formatted);
                        break;
                }
            }
            catch (ArgumentException)
            {
                throw new OutOfDataException(
                    "Element {0}: Not enough data left to create a {1} byte number ({2} bytes left)",
                    Name, Units, data.Length);
            }

            return result;
        }

        /// <summary>
        /// Interpret data as a number according to this element's definition
        /// and uses the specified enumeration to use as a string value.
        /// </summary>
        /// <param name="def">Enumeration to use</param>
        /// <param name="data">Raw data</param>
        /// <returns></returns>
        /// <exception cref="OutOfDataException">Thrown is data does not allow for interpreting
        /// in the specified Units or Width</exception>
        public string TryFormatEnumeration(Enumeration def, byte[] data)
        {
            try
            {
                var number = new Number(this, data);
                return def.Values.TryGetValue(number.si, out var name)
                    ? name
                    : $"{number.si} is not defined in {def.Name}";
            }
            catch (ArgumentException)
            {
                throw new OutOfDataException(
                    "Element {0}: Not enough data left to create a {1} byte number ({2} bytes left)",
                    Name, Units, data.Length);
            }
        }

        /// <summary>
        /// Format number using the current PrintFormat
        /// </summary>
        /// <param name="number">Number to format</param>
        /// <returns>Formatted string</returns>
        private string FormatNumber(Number number)
        {
            string result;

            switch (PrintFormat)
            {
                case Bender.PrintFormat.Binary:
                    // Make sure every byte has 8 places, 0 filled if needed
                    var binary = Convert.ToString(number.sl, 2).PadLeft(Units * 8, '0');
                    result = $"b{binary}";
                    break;

                case Bender.PrintFormat.Octal:
                    result = $"O{Convert.ToString(number.sl, 8)}";
                    break;

                case Bender.PrintFormat.Decimal:
                    result = number.sl.ToString();
                    break;

                case Bender.PrintFormat.Hex:
                case Bender.PrintFormat.BigInt:
                    var prefix = PrintFormat == Bender.PrintFormat.Hex ? "0x" : "";
                    var width = (Units * 2).NextPowerOf2();
                    var hex = Convert.ToString(number.sl, 16).PadLeft(width, '0').ToUpper();
                    result = $"{prefix}{hex}";
                    break;

                case Bender.PrintFormat.Float:
                    result = Units switch
                    {
                        // Reinterpret data as floating point
                        4 => number.fs.ToString("F6"),
                        8 => number.fd.ToString("F6"),
                        _ => "Malformed float. Width must be 4 or 8 bytes"
                    };

                    break;

                case Bender.PrintFormat.Ascii:
                case Bender.PrintFormat.Unicode:
                    throw new ParseException("Cannot format numbers as a string type. This is bug");

                default:
                    result = $"Unsupported format: {PrintFormat}";
                    break;
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
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}, Units: {Units}, Format: {PrintFormat}, LE: {IsLittleEndian}, Elide: {Elide}";
        }
    }
}