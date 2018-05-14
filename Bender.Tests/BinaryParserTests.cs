using Xunit;

namespace BenderLib.Tests
{
    public class BinaryParserTests
    {
        [Fact]
        public void TestParseMatchingSpec()
        {
            var spec = new SpecParser().Parse(DataFile.From(Properties.Resources.simple_layout));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            Assert.NotNull(bender);
        }

        [Fact]
        public void TestParseMissingMatrix()
        {
            var spec = new SpecParser().Parse(DataFile.From(Properties.Resources.test_missing_matrix));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            Assert.NotNull(bender);
        }

        [Fact]
        public void TestParseStrings()
        {
            var spec = new SpecParser().Parse(DataFile.From(Properties.Resources.test_strings));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            Assert.NotNull(bender);
        }

        [Fact]
        public void TestParseFloats()
        {
            var spec = new SpecParser().Parse(DataFile.From(Properties.Resources.test_floats));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            Assert.NotNull(bender);
        }
    }
}
