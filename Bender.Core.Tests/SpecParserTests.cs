using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bender.Core.Tests
{
    public class SpecParserTests
    {
        [Fact]
        public void TestInvalidFile()
        {
            var df = DataFile.FromASCII(TestData.TestInvalidYAML);
            var parser = new SpecParser();
            Assert.Throws<ParseException>(() => parser.Parse(df));
        }

        [Fact]
        public void TestValidFile()
        {
            var df = DataFile.From(Properties.Resources.simple_layout);
            var parser = new SpecParser();
            var result = parser.Parse(df);

            Assert.False(string.IsNullOrEmpty(result.ToString()));
            Assert.Equal("bender.v1", result.Format);
            Assert.Equal("simple_layout", result.Name);
            Assert.Equal("A simple sample binary layout descriptor that can span multiple lines\n", result.Description);
            Assert.Equal(new List<string> { "test", "simple"}, result.Extensions);
            Assert.Equal(9, result.Elements.Count);

            var el = new Element
            {
                IsLittleEndian = true,
                Elide = false,
                PrintFormat = Bender.PrintFormat.Ascii,
                Units = 4,
                Name = "Undefined",
                Matrix = null,
                IsSigned = false
            };
            Assert.Equal(el, result.BaseElement);

            Assert.False(string.IsNullOrEmpty(el.ToString()));
            Assert.Equal(el.GetHashCode(), result.BaseElement.GetHashCode());
        }
    }
}
