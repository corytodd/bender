// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global - This is a library class, consumers may use all properties
namespace Bender.Core
{
    using System.Collections.Generic;
    using System.Text;
    using Layouts;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents YAML specification for Bender files
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
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
        /// List of named structures
        /// </summary>
		public IList<Structure> Structures { get; set; } = new List<Structure>();
        
        /// <summary>
        /// List of defined enumeration mappings
        /// </summary>
        public IList<Enumeration> Enumerations { get; set; } = new List<Enumeration>();

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
            FormatLayout(BaseElement, sb);

            
            sb.AppendLine("Structures:");
            FormatLayouts(Structures, sb);
            
            sb.AppendLine("Enumerations:");
            FormatLayouts(Enumerations, sb);

            sb.AppendLine("Elements:");
            FormatLayouts(Elements, sb);

            sb.AppendLine("Layout:");
            foreach (var str in Layout)
            {
                sb.AppendFormat("\t{0}\n", str);
            }
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Format a layout
        /// </summary>
        /// <param name="layout">Layout to format</param>
        /// <param name="sbOut">Receives formatted layout</param>
        private static void FormatLayout(ILayout layout, StringBuilder sbOut)
        {
            foreach (var str in layout.EnumerateLayout())
            {
                sbOut.AppendFormat("\t{0}\n", str);
            }
            sbOut.AppendLine();
        }
        
        /// <summary>
        /// Format a collection of layouts and separate with a newline
        /// </summary>
        /// <param name="layouts">Layouts to format</param>
        /// <param name="sbOut">Receives formatted layout</param>
        private static void FormatLayouts(IEnumerable<ILayout> layouts, StringBuilder sbOut)
        {
            foreach (var layout in layouts)
            {
                FormatLayout(layout, sbOut);
            }
            sbOut.AppendLine();
        }
    }
}
