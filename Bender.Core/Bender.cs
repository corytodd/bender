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
    }
}
