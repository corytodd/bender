namespace Bender.Core.Nodes
{
    using System.IO;
    using Layouts;
    using Rendering;

    /// <summary>
    ///     Specialized string element type provides an <see cref="IRenderable"/>
    ///     version of the native string type.
    /// </summary>
    public class BString : BaseNode
    {
        /// <summary>
        ///     Create a new BString
        /// </summary>
        /// <param name="el">Parent</param>
        /// <param name="value">String value</param>
        public BString(Element el, string value) : base(el)
        {
            Value = value;
        }

        /// <summary>
        ///     String value
        /// </summary>
        public string Value { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} : {Value}";
        }

        /// <inheritdoc />
        public override void Render(StreamWriter stream)
        {
            stream.Write(Format());
        }
    }
}