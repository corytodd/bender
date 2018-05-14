namespace BenderLib
{
    using YamlDotNet.Serialization;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization.NamingConventions;

    /// <summary>
    /// Parse a specication file that is in YAML format
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
                using (var reader = file.AsStringReader())
                {
                    var parser = new MergingParser(new Parser(reader));
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(new CamelCaseNamingConvention())
                        .Build();

                    return deserializer.Deserialize<SpecFile>(parser);
                }
            }
            catch (SemanticErrorException ex)
            {                
                throw new ParseException(ex.Message);
            }
        }
    }
}
