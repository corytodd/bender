namespace Bender.Core
{
    using System.IO;
    using Rendering;

    /// <summary>
    ///     An IRenderable wrapper around string. For lack
    ///     of a better term, Phrase just means string.
    /// </summary>
    public record Phrase : IRenderable
    {
        /// <summary>
        ///     Create a new phrase
        /// </summary>
        /// <param name="value">phrase value</param>
        public Phrase(string value)
        {
            Value = value;
        }

        /// <summary>
        ///     String value
        /// </summary>
        public string Value { get; }

        /// <inheritdoc />
        public string Format()
        {
            return Value;
        }

        /// <inheritdoc />
        public void Render(StreamWriter stream)
        {
            stream.Write(Value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Format();
        }
    }
}