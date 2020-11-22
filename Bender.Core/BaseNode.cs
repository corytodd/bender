namespace Bender.Core
{
    using System.IO;

    public abstract class BaseNode : BNode
    {
        protected BaseNode(Element el)
        {
            El = el;
            Name = el?.Name;
        }

        /// <inheritdoc />
        public Element El { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public virtual string Format(string format)
        {
            return ToString();
        }

        /// <inheritdoc />
        public abstract void Print(StreamWriter writer);
    }
}