namespace Bender.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    /// <summary>
    /// Represents a format type arranging data into rows and columns
    /// </summary>
    public class Matrix : ILayout
    {
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
        /// <param name="elementFormatter">Converts extracted data into a formatted string</param>
        /// <returns>List of rows, formatted as strings</returns>
        public IEnumerable<string> TryFormat(Element el, byte[] data, Bender.FormatElement elementFormatter)
        {
            if (el is null)
            {
                throw new ArgumentNullException(nameof(el));
            }

            if (data is null)
            {
                throw new ArgumentException(nameof(el));
            }

            if (elementFormatter is null)
            {
                throw new ArgumentNullException(nameof(elementFormatter));
            }
            
            if (Units == 0 || data.Length == 0)
            {
                return Enumerable.Empty<string>();
            }

            var value = new List<string>();
            var sb = new StringBuilder();
            sb.Append("[ ");

            var totalVars = data.Length / Units;

            // Chop data into payload.unit bytes to produce an output similar to
            //
            // [ ... numbers ... ]
            // [ ... numbers ... ]
            //
            var cols = 0;
            var count = 0;
            foreach (var unit in data.AsChunks(Units))
            {
                ++count;

                sb.AppendFormat("{0} ", elementFormatter.Invoke(el, unit));
                if (++cols % Columns != 0)
                {
                    continue;
                }

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

            sb.AppendFormat("Columns: {0}\n", Columns);
            sb.AppendFormat("Units: {0}\n", Units);

            return sb.ToString();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Units: {Units}, Columns: {Columns}";
        }
    }
}
