using System;
using System.Collections.Generic;
using System.Linq;
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

            Assert.False(string.IsNullOrEmpty(result.ToString()));
            Assert.Equal("bender.v1", result.Format);
            Assert.Equal("simple_layout", result.Name);
            Assert.Equal("A simple sample binary layout descriptor that can span multiple lines\n", result.Description);
            Assert.Equal(new List<string> { "test", "simple"}, result.Extensions);
            Assert.Equal(8, result.Elements.Count);
            Assert.Equal(2, result.Matrices.Count);

            var el = new Element
            {
                LittleEndian = true,
                Elide = false,
                IsReadOnly = false,
                Format = ElementFormat.ASCII,
                Units = 4,
                Name = "Undefined",
                Matrix = null,
                IsSigned = false
            };
            Assert.Equal(el, result.BaseElement);

            Assert.False(string.IsNullOrEmpty(el.ToString()));
            Assert.Equal(el.GetHashCode(), result.BaseElement.GetHashCode());
        }

        [Fact]
        public void TestParseDeferred()
        {
            var spec = new SpecParser().Parse(DataFile.From(Properties.Resources.test_deferred));
            Assert.NotNull(spec.Deferreds);
            Assert.Equal(1, spec.Deferreds.Count);

            var def = spec.Deferreds.First();
            Assert.Equal("neat_blob", def.Name);
            Assert.Equal(4, def.SizeUnits);
            Assert.Equal(4, def.OffsetUnits);

            // Make sure that the element in this file has the right deferred name
            var el = spec.Elements.FirstOrDefault(e => def.Name.Equals(e.Deferred));
            Assert.NotNull(el);
        }
    }
}
