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
