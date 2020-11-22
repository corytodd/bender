namespace Bender.Core.Nodes
{
    using System.IO;
    using Layouts;

    public class BPrimitive<T> : BaseNode
    {
        public BPrimitive(Element el, T value) : base(el)
        {
            Value = value;
        }
        
        public T Value { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}:{Value}";
        }

        /// <inheritdoc />
        public override void Print(StreamWriter writer)
        {
            writer.WriteLine(this);
        }
    }
}