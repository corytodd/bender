namespace Bender.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Maps a numeric value to a string representation
    /// </summary>
    public class Enumeration
    {
        /// <summary>
        /// Get or Set name of this enumerate
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Get or Set the name lookup table
        /// </summary>
        public Dictionary<int, string> Values { get; set; }
    }
}