namespace Bender.Core
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
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// Friendly name for what this bender is describing
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of bender
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// List of file extension known to associate with this specification
        /// </summary>
        public IList<string> Extensions { get; set; } = new List<string>();

        /// <summary>
        /// Default based element
        /// </summary>
        [YamlMember(Alias = "base_element", ApplyNamingConventions = false)]
        public Element BaseElement { get; set; } = new Element();

        /// <summary>
        /// Default based element
        /// </summary>
        [YamlMember(Alias = "base_matrix", ApplyNamingConventions = false)]
        public Matrix BaseMatrix { get; set; } = new Matrix();
        
        /// <summary>
        /// List of named structures
        /// </summary>
        /// <value>The structures.</value>
		public IList<Structure> Structures { get; set; } = new List<Structure>();

        /// <summary>
        /// Ordered list of elements in this file
        /// </summary>
        public IList<Element> Elements { get; set; } = new List<Element>();

        /// <summary>
        /// Gets or Sets ordered layout tags
        /// </summary>
        public IList<string> Layout { get; set; } = new List<string>();

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

            sb.AppendLine("Default Matrix:");
            foreach (var str in BaseMatrix.EnumerateLayout())
            {
                sb.AppendFormat("\t{0}\n", str);
            }
            sb.AppendLine();
            
            sb.AppendLine("Structures:");
            foreach (var m in Structures)
            {
                foreach (var str in m.EnumerateLayout())
                {
                    sb.AppendFormat("\t{0}\n", str);
                }
                sb.AppendLine();
            }

            sb.AppendLine("Elements:");
            foreach(var el in Elements)
            {
                foreach(var str in el.EnumerateLayout()) {
                    sb.AppendFormat("\t{0}\n", str);
                }
                sb.AppendLine();
            }

            sb.AppendLine("Layout:");
            foreach (var str in Layout)
            {
                sb.AppendFormat("\t{0}\n", str);
            }
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
