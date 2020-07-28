namespace Bender.Core
{
    using System.IO;
    using System.Text;

    public class BPrimitive : BNode
    {
        public BPrimitive(string name, string value)
        {
            Name = name;
            Value = value;
        }
        
        public string Name { get; }
        
        public string Value { get; }

        public override string ToString()
        {
            return $"{Name}:{Value}";
        }

        public void Print(StreamWriter writer)
        {
            writer.WriteLine(this);
        }
    }
}