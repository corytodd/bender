namespace Bender.Core
{
    using System.Collections.Generic;
    
    /// <summary>
    /// A 
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// Formats data into a string defined by the rules in element
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="data">Data to format</param>
        /// <returns>Formatted string</returns>
        public delegate string Formatter(Element el, byte[] data);

        /// <summary>
        /// Extracts data from element definition and formats using provided formatter
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="data">Data to format</param>
        /// <param name="formatter">Converts extracted data into a formatted string</param>
        /// <returns>Formatted data</returns>
        public IEnumerable<string> TryFormat(Element el, byte[] data, Formatter formatter);
    }
}