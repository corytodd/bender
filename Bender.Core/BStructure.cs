namespace Bender.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class BStructure : BNode
    {
        public BStructure(string name)
        {
            Name = name;
            Fields = new List<BNode>();
        }

        public string Name { get; }

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

        public void Print(StreamWriter writer)
        {
            writer.WriteLine(this);
        }
    }
}