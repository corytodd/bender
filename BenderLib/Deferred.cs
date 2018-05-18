namespace BenderLib
{
    using System.Collections.Generic;
    using System.Text;
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

        /// <summary>
        /// Generator yields each line from ToString()
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> EnumerateLayout()
        {
            var content = ToString().Split('\n');
            foreach (var str in content)
            {
                yield return str;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Name: {0}\n", Name);
            sb.AppendFormat("Size Units: {0}\n", SizeUnits);
            sb.AppendFormat("Offset Units: {0}\n", OffsetUnits);

            return sb.ToString();
        }
    }
}
