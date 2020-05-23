namespace Bender.Core
{
    using System.IO;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Structure represents a struct in binary form
    /// </summary>
    public class Structure : IElement
    {
        /// <summary>
        /// Gets or Sets name of this structure
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets ordered list of elements in this structure
        /// </summary>
        public IList<Element> Elements { get; set; }

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
        /// Formats data into an ordered list of matrix rows. Each row is
        /// formatted using the rules defined in element.
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="data">Data to format</param>
        /// <param name="formatter">Converts extracted data into a formatted string</param>
        /// <returns>List of rows, formatted as strings</returns>
        public IEnumerable<string> TryFormat(Element el, byte[] data, IElement.Formatter formatter)
        {
            var result = new List<string>(Elements.Count);

            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            foreach (var childEl in Elements)
            {
                var field = reader.ReadBytes(childEl.Units);
                result.Add($"[ {childEl.Name}: {formatter.Invoke(childEl, field)} ]");
            }

            return result;
        }

        /// <summary>
        ///     Returns all properties as newline delimited string
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Name: {0}\n", Name);

            sb.AppendLine("\tElements:");
            foreach (var el in Elements)
            {
                foreach (var str in el.EnumerateLayout())
                {
                    sb.AppendFormat("\t\t{0}\n", str);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}