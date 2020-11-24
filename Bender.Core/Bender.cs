namespace Bender.Core
{
    using Nodes;

    /// <summary>
    /// A fully parsed binary file
    /// </summary>
    public class Bender
    {
        /// <summary>
        /// Create a new Bender with this reference spec
        /// </summary>
        public Bender(BNode node)
        {
            Tree = new ParseTree<BNode>(node);
        }

        /// <summary>
        /// Logical structure of binary file
        /// </summary>
        public ParseTree<BNode> Tree { get; }

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