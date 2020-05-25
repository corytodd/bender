namespace Bender.Core
{
    /// <summary>
    /// Supported element formats
    /// </summary>
    public enum ElementFormat
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
        ASCII,

        /// <summary>
        /// Format value as a UTF-16 string
        /// </summary>
        UTF16,

        /// <summary>
        /// Format as hex string, no prefix
        /// </summary>
        HexString,
        
        /// <summary>
        /// Format as single floating point precision
        /// </summary>
        Single,
        
        /// <summary>
        /// Format as double floating point precision
        /// </summary>
        Double,
    }
}