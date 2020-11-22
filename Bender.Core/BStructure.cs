namespace Bender.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class BStructure : BaseNode
    {
        public BStructure(Element el) : base(el)
        {
            Fields = new List<BNode>();
        }

        public List<BNode> Fields { get; }

        public override string ToString()
        {
            // TODO add check to prevent infinite loop
            var sb = new StringBuilder();
            sb.Append($"{Name}:[");
            sb.Append(string.Join(",", Fields));
            sb.Append("]");

            return sb.ToString();
        }

        /// <inheritdoc />
        public override void Print(StreamWriter writer)
        {
            writer.WriteLine(this);
        }
    }
}