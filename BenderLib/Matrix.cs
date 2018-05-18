namespace BenderLib
{
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    /// <summary>
    /// Represents a format type arranging data into rows and columns
    /// </summary>
    public class Matrix
    {
        /// <summary>
        /// Formats data into a string defined by the rules in element
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="data">Data to format</param>
        /// <returns>Formatted string</returns>
        public delegate string Formatter(Element el, byte[] data);

        /// <summary>
        /// Gets or Sets matrix name 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets count of columns in this matrix
        /// </summary>
        public int Columns { get; set; }

        /// <summary>
        /// Gets or Sets the units, or how many bytes are used for each
        /// digit in the matrix.
        /// </summary>
        public int Units { get; set; }

        /// <summary>
        /// Formats data into an ordered list of matrix rows. Each row is
        /// formatted using the rules defined in element.
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="data">Data to format</param>
        /// <param name="formatter">Converts extracted data into a formatted string</param>
        /// <returns>List of rows, formatted as strings</returns>
        public IEnumerable<string> Format(Element el, byte[] data, Formatter formatter)
        {
            if (Units == 0)
            {
                return Enumerable.Empty<string>();
            }

            var value = new List<string>();
            var sb = new StringBuilder();
            sb.Append("[ ");

            var totalVars = data.Length / Units;

            // Chop data into payload.unit bytes
            var cols = 0;
            var count = 0;
            foreach (var unit in data.AsChunks(Units))
            {
                ++count;

                sb.AppendFormat("{0} ", formatter.Invoke(el, unit));
                if (++cols % Columns != 0) continue;

                sb.Append("]");
                value.Add(sb.ToString());
                sb.Clear();

                if (count != totalVars)
                {
                    sb.Append("[ ");
                }
            }

            return value;
        }

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

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Name: {0}\n", Name);
            sb.AppendFormat("Columns: {0}\n", Columns);
            sb.AppendFormat("Units: {0}\n", Units);

            return sb.ToString();
        }
    }
}
