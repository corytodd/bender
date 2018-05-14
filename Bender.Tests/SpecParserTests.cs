using System.Collections.Generic;
using Xunit;

namespace BenderLib.Tests
{
    public class SpecParserTests
    {
        [Fact]
        public void TestInvalidFile()
        {
            var df = DataFile.From(Properties.Resources.test_invalid);
            var parser = new SpecParser();
            Assert.Throws<ParseException>(() => parser.Parse(df));
        }

        [Fact]
        public void TestValidFile()
        {
            var df = DataFile.From(Properties.Resources.simple_layout);
            var parser = new SpecParser();
            var result = parser.Parse(df);

            Assert.Equal("bender.v1", result.Format);
            Assert.Equal("simple_layout", result.Name);
            Assert.Equal("A simple sample binary layout descriptor that can span multiple lines\n", result.Description);
            Assert.Equal(new List<string> { "simple"}, result.Extensions);
            Assert.Equal(8, result.Elements.Count);
            Assert.Equal(2, result.Matrices.Count);

            var el = new Element
            {
                LittleEndian = true,
                Elide = false,
                IsReadOnly = false,
                Format = ElementFormat.ASCII,
                Width = 4,
                Name = "Undefined",
                Matrix = null,
                IsSigned = false
            };
            Assert.Equal(el, result.Base);

            Assert.False(string.IsNullOrEmpty(el.ToString()));
            Assert.Equal(el.GetHashCode(), result.Base.GetHashCode());
        }
    }
}
