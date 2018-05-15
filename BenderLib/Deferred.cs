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
        /// Width of bytes of size field
        /// </summary>
        [YamlMember(Alias = "size_width", ApplyNamingConventions = false)]
        public int SizeWidth { get; set; }

        /// <summary>
        /// Width in bytes of the offset field
        /// </summary>
        [YamlMember(Alias = "offset_width", ApplyNamingConventions = false)]
        public int OffsetWidth { get; set; }
    }
}
