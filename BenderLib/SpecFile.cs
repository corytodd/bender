namespace BenderLib
{
    using System.Collections.Generic;
    using System.Text;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents YAML specification for Bender files
    /// </summary>
    public class SpecFile
    {
        /// <summary>
        /// Identify this file as a Bender file
        /// Will always be in the format: bender.vX
        /// where X is the schema revision
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Friendly name for what this bender is describing
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of bender
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of file extension known to associate with this specification
        /// </summary>
        public IList<string> Extensions { get; set; }

        /// <summary>
        /// Default based element
        /// </summary>
        [YamlMember(Alias = "base_element", ApplyNamingConventions = false)]
        public Element BaseElement { get; set; }

        /// <summary>
        /// Default based element
        /// </summary>
        [YamlMember(Alias = "base_matrix", ApplyNamingConventions = false)]
        public Matrix BaseMatrix { get; set; }

        /// <summary>
        /// List of matrix formatters
        /// </summary>
        /// <value>The matrices.</value>
        public IList<Matrix> Matrices { get; set; }

        /// <summary>
        /// List of named deferred objects
        /// </summary>
        /// <value>The deferreds.</value>
        public IList<Deferred> Deferreds { get; set; }

        /// <summary>
        /// List of named structures
        /// </summary>
        /// <value>The structures.</value>
		public IList<Structure> Structures { get; set; }

        /// <summary>
        /// Ordered list of elements in this file
        /// </summary>
        public IList<Element> Elements { get; set; }

        /// <summary>
        /// Gets or Sets ordered layout tags
        /// </summary>
        public IList<string> Layout { get; set; }

        /// <summary>
        /// Pretty print in tabular form:
        /// Name : Description
        /// -------------------------------
        /// Extensions:
        ///    - .something
        ///    - .other
        /// Default Element:
        ///    key : value
        ///    key : value
        /// Default Matrix:
        ///    key : value
        ///    key : value       
        /// ...
        /// Elements:
        ///   ...
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            sb.AppendFormat(" : {0}\n", Description);
            sb.AppendLine(new string('-', 80));

            sb.AppendLine("Extensions:");
            foreach(var ext in Extensions)
            {
                sb.AppendFormat("\t - .{0}\n", ext);
            }
            sb.AppendLine();

            sb.AppendLine("Default Element:");
            foreach (var str in BaseElement.EnumerateLayout())
            {
                sb.AppendFormat("\t{0}\n", str);
            }
            sb.AppendLine();

            sb.AppendLine("Elements:");
            foreach(var el in Elements)
            {
                foreach(var str in el.EnumerateLayout()) {
                    sb.AppendFormat("\t{0}\n", str);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
