using Xunit;

namespace BenderLib.Tests
{
    public class BinaryParserTests
    {
        [Fact]
        public void TestParseMatchingSpec()
        {
            var spec = new SpecParser().Parse(DataFile.FromASCII(TestData.SimpleTest));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            Assert.NotNull(bender);
        }

        [Fact]
        public void TestParseMissingMatrix()
        {
            var spec = new SpecParser().Parse(DataFile.FromASCII(TestData.TestMatrices));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            Assert.NotNull(bender);
        }

        [Fact]
        public void TestParseStrings()
        {
            var spec = new SpecParser().Parse(DataFile.FromASCII(TestData.StringTest));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            Assert.NotNull(bender);
        }

        [Fact]
        public void TestParseDeferred()
        {
            var spec = new SpecParser().Parse(DataFile.FromASCII(TestData.TestDeferred));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.test_deferred_binary));
            Assert.NotNull(bender);
        }
    }
}
