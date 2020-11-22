namespace Bender.Core.Nodes
{
    using System.IO;
    using Layouts;
    using Rendering;

    public class BPrimitive<T> : BaseNode where T : IRenderable
    {
        public BPrimitive(Element el, T value) : base(el)
        {
            Value = value;
        }
        
        public T Value { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} : {Value.Format()}";
        }

        /// <inheritdoc />
        public override void Render(StreamWriter stream)
        {
            stream.Write(this);
        }
    }
}