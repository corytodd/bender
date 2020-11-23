// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global - This is a library class, consumers may use all properties
// ReSharper disable UnusedAutoPropertyAccessor.Global - This is a library class, consumers may use all properties

namespace Bender.Core.Layouts
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Structure represents a struct in binary form
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global - This is a library class, consumers may instantiate this
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

        /// <summary>
        /// Returns the size in bytes of this structure
        /// </summary>
        public int Size => Elements.Select(x => x.Size).Sum();

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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}, Field Count: {Elements.Count}";
        }
    }
}