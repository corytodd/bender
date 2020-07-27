namespace Bender.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// A fully parsed binary file
    /// </summary>
    public class Bender
    {
        /// <summary>
        /// Contains one complete, formatted field entry that can span
        /// rows and maintain column alignment.
        /// { "field name", "decode value(s) that"
        ///                 "can span lines" }
        /// </summary>
        public struct FormattedField
        {
            /// <summary>
            ///     Field key
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            ///     Field values
            /// </summary>
            public IList<string> Value { get; set; }

            /// <summary>
            /// Helper to generate a new formatted field for this element
            /// </summary>
            /// <param name="el">Element instance</param>
            /// <param name="message">Message to assign as value</param>
            /// <returns>New formatted field</returns>
            public static FormattedField From(Element el, string message)
            {
                return From(el.Name, message);
            }

            /// <summary>
            /// Helper to generate a new formatted field for this name
            /// </summary>
            /// <param name="name">Field name</param>
            /// <param name="message">Message to assign as value</param>
            /// <returns>New formatted field</returns>
            public static FormattedField From(string name, string message)
            {
                return new FormattedField
                {
                    Name = name,
                    Value = new[] {message}
                };
            }
        }

        /// <summary>
        /// Create a new Bender with this reference spec
        /// </summary>
        public Bender()
        {
            Tree = new ParseTree();
        }
        
        public ParseTree Tree { get; }
        
        /// <summary>
        /// Supported element formats
        /// </summary>
        public enum PrintFormat
        {
            /// <summary>
            /// Format value as binary with 'b' prefix and
            /// 8 characters per byte (left zero padded). LSB
            /// is on the right end of the string.
            /// e.g. b011100101
            /// </summary>
            Binary,

            /// <summary>
            /// Format value as octal with 'o' prefix
            /// </summary>
            Octal,

            /// <summary>
            /// Format as decimal
            /// </summary>
            Decimal,

            /// <summary>
            /// Format value as hex with '0x' prefix
            /// </summary>
            Hex,

            /// <summary>
            /// Parse as ASCII text
            /// </summary>
            Ascii,

            /// <summary>
            /// Format value as a Unicode string
            /// </summary>
            Unicode,

            /// <summary>
            /// Number with arbitrary count of digits
            /// </summary>
            BigInt,

            /// <summary>
            /// Format as single or double floating point precision
            /// Element width must be 4 or 8 bytes, respectively 
            /// </summary>
            Float
        }
    }
}