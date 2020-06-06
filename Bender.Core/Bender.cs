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
        }

        /// <summary>
        /// Create a new Bender with this reference spec
        /// </summary>
        public Bender()
        {
            FormattedFields = new List<FormattedField>();
        }

        /// <summary>
        /// Returns list of formatted data extracted from binary
        /// </summary>
        public List<FormattedField> FormattedFields { get; }

        /// <summary>
        /// Formats data into a string defined by the rules in element
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="buff">Data to format</param>
        /// <returns>Formatted string</returns>
        public delegate FormattedField FormatElement(Element el, byte[] buff);

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
