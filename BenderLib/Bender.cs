namespace BenderLib
{
    using System.Collections.Generic;

    /// <summary>
    /// A fully parsed binary file
    /// </summary>
    public class Bender
    {
        /// <summary>
        /// Contains one complete, formatted field entry
        /// { "field name", "decode value(s) that"
        ///                 "can span lines" }
        /// </summary>
        public struct FormattedField
        {
            public string Name { get; set; }

            public IList<string> Value { get; set; }
        }

        /// <summary>
        /// Create a new Bender with this reference spec
        /// </summary>
        /// <param name="spec"></param>
        public Bender(SpecFile spec)
        {
            FormattedFields = new List<FormattedField>();
        }

        /// <summary>
        /// Returns list of formatted data extracted from binary
        /// </summary>
        public IList<FormattedField> FormattedFields { get; private set; }


    }
}
