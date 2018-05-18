namespace BenderLib
{
	using System.Collections.Generic;
	using System.Text;

    public class Structure
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
