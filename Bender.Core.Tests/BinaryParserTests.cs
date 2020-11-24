using Xunit;

namespace Bender.Core.Tests
{
    public class BinaryParserTests
    {
        [Fact]
        public void TestParseMatchingSpec()
        {
            var spec = SpecFile.Parse(DataFile.FromAscii(TestData.SimpleTest));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            Assert.NotNull(bender);
        }

        [Fact]
        public void TestParseMissingMatrix()
        {
            var spec = SpecFile.Parse(DataFile.FromAscii(TestData.TestMatrices));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            Assert.NotNull(bender);
        }

        [Fact]
        public void TestParseStrings()
        {
            var spec = SpecFile.Parse(DataFile.FromAscii(TestData.StringTest));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            Assert.NotNull(bender);
        }

        [Fact]
        public void TestParseDeferred()
        {
            var spec = SpecFile.Parse(DataFile.FromAscii(TestData.TestDeferred));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.test_deferred_binary));
            Assert.NotNull(bender);
        }
    }
}
