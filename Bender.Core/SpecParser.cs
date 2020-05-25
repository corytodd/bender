namespace Bender.Core
{
    using System.Text;
    using System.Collections.Generic;
    using YamlDotNet.Serialization;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization.NamingConventions;

    /// <summary>
    /// Parse a specification file that is in YAML format
    /// </summary>
    public class SpecParser
    {
        /// <summary>
        /// Parses DataFile as a SpecFile
        /// </summary>
        /// <param name="file">File to read</param>
        /// <exception cref="ParseException">Raised if file cannot be parsed</exception>
        /// <returns>Parsed spec file</returns>
        public SpecFile Parse(DataFile file)
        {
            try
            {
                using var reader = file.AsStringReader();
                var parser = new MergingParser(new Parser(reader));
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                return deserializer.Deserialize<SpecFile>(parser);
            }
            catch (YamlException ex)
            {
                var sb = new StringBuilder(ex.Message);
                var inner = ex.InnerException;
                while (inner != null)
                {
                    sb.AppendLine(inner.Message);
                    inner = inner.InnerException;
                }

                throw new ParseException(sb.ToString());
            }
            catch (KeyNotFoundException ex)
            {
                var err = $"Malformed YAML, try running through an online validator: {ex.Message}";
                throw new ParseException(err);
            }
        }
    }
}