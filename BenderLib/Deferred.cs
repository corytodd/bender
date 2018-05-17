namespace BenderLib
{
    using YamlDotNet.Serialization;

    /// <summary>
    /// Decribes data that should be read from another location
    /// </summary>
    public class Deferred
    {
        /// <summary>
        /// Gets or Sets name for this deferred block
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Units of bytes of size field
        /// </summary>
        [YamlMember(Alias = "size_units", ApplyNamingConventions = false)]
        public int SizeUnits { get; set; }

        /// <summary>
        /// Units in bytes of the offset field
        /// </summary>
        [YamlMember(Alias = "offset_units", ApplyNamingConventions = false)]
        public int OffsetUnits { get; set; }
    }
}
