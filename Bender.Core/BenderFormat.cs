namespace Bender.Core
{
    /// <summary>
    /// Bender formatting
    /// </summary>
    public class BenderFormat
    {
        /// <summary>
        /// Formats data into a string defined by the rules in element
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="data">Data to format</param>
        /// <returns>Formatted string</returns>
        public delegate string Formatter(Element el, byte[] data);
    }
}