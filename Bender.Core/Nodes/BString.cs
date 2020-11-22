namespace Bender.Core.Nodes
{
    using System.Diagnostics;
    using System.IO;
    using Layouts;
    
    [DebuggerDisplay("{Name}")]
    public class BString : BNode
    {
        public BString(Element el, string value)
        {
            El = el;
            Name = el.Name;
            Value = value;
        }

        public Element El { get; }

        public string Name { get; }

        public string Value { get; }

        /// <inheritdoc />
        public string Format()
        {
            return $"{Name} : {Value}";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Format();
        }

        /// <inheritdoc />
        public void Render(StreamWriter stream)
        {
            stream.Write(Format());
        }
    }
}