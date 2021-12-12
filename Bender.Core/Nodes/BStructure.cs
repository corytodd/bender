namespace Bender.Core.Nodes
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Layouts;

    /// <summary>
    ///     A recursive node structure
    /// </summary>
    public class BStructure : BaseNode
    {
        /// <summary>
        ///     Create a new structure
        /// </summary>
        /// <param name="el">Parent</param>
        public BStructure(Element el) : base(el)
        {
            Fields = new List<BNode>();
        }

        /// <summary>
        ///     Top-level struct members
        /// </summary>
        public List<BNode> Fields { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Name} : [");
            sb.Append(string.Join(", ", Fields.Select(n => n.Name)));
            sb.Append("]");

            return sb.ToString();
        }
    }
}