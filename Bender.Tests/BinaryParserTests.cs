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
            var spec = new SpecParser().Parse(DataFile.From(Properties.Resources.test_matrices));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            Assert.NotNull(bender);
        }

        [Fact]
        public void TestParseEmptyMatrices()
        {
            var spec = new SpecParser().Parse(DataFile.From(Properties.Resources.test_empty_matrices));
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
    }
}
