namespace Bender.Core
{
    using System.Collections.Generic;
    using System.Text;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Describes data that should be read from another location
    /// </summary>
    public class Deferred
    {
        /// <summary>
        /// Gets or Sets name for this deferred block
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Size of deferred object in bytes
        /// </summary>
        [YamlMember(Alias = "size_bytes", ApplyNamingConventions = false)]
        public int SizeBytes { get; set; }

        /// <summary>
        /// Starting address of deferred object relative to start of file
        /// </summary>
        [YamlMember(Alias = "offset_bytes", ApplyNamingConventions = false)]
        public int OffsetBytes { get; set; }

        /// <summary>
        /// Generator yields each line from ToString()
        /// </summary>
        public IEnumerable<string> EnumerateLayout()
        {
            var content = ToString().Split('\n');
            foreach (var str in content)
            {
                yield return str;
            }
        }

        /// <summary>
        ///     Returns all properties as newline delimited string
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Name: {0}\n", Name);
            sb.AppendFormat("Size Bytes: {0}\n", SizeBytes);
            sb.AppendFormat("Offset Bytes: {0}\n", OffsetBytes);

            return sb.ToString();
        }
    }
}
