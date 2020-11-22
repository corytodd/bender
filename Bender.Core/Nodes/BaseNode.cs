namespace Bender.Core.Nodes
{
    using System.Diagnostics;
    using System.IO;
    using Layouts;

    [DebuggerDisplay("{Name}")]
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
        public virtual string Format()
        {
            return ToString();
        }

        /// <inheritdoc />
        public abstract void Render(StreamWriter stream);
    }
}