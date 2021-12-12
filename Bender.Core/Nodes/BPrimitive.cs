namespace Bender.Core.Nodes
{
    using Layouts;
    using Rendering;

    /// <summary>
    ///     A single element
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    public class BPrimitive<T> : BaseNode where T : IRenderable
    {
        /// <summary>
        ///     Create a new primitive
        /// </summary>
        /// <param name="el">Parent</param>
        /// <param name="value">Element value</param>
        public BPrimitive(Element el, T value) : base(el)
        {
            Value = value;
        }

        /// <summary>
        ///     Element value
        /// </summary>
        public T Value { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} : {Value.Format()}";
        }
    }
}