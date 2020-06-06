namespace Bender.Core
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Structure represents a struct in binary form
    /// </summary>
    public class Structure : ILayout
    {
        /// <summary>
        /// Gets or Sets name of this structure
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets ordered list of elements in this structure
        /// </summary>
        public IList<Element> Elements { get; set; } = new List<Element>();

        /// <inheritdoc />
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

            sb.AppendLine("\tElements:");
            if (!(Elements is null))
            {
                foreach (var el in Elements)
                {
                    foreach (var str in el.EnumerateLayout())
                    {
                        sb.AppendFormat("\t\t{0}\n", str);
                    }

                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}