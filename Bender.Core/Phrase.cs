namespace Bender.Core
{
    using System.IO;
    using Rendering;

    /// <summary>
    ///     An IRenderable wrapper around string. For lack
    ///     of a better term, Phrase just means string.
    /// </summary>
    public class Phrase : IRenderable
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
        protected bool Equals(Phrase other)
        {
            return Value == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Phrase) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(Phrase? left, Phrase? right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(Phrase? left, Phrase? right)
        {
            return !Equals(left, right);
        }
    }
}