using Xunit;

namespace Bender.Core.Tests
{
    public class MatrixTests
    {
        [Fact]
        public void TestFormatBytes()
        {
            var el = new Element
            {
                Name = "Test",
                Units = 8,
                PrintFormat = Bender.PrintFormat.Ascii,
                Matrix = null,
                IsLittleEndian = true,
                IsSigned = true,
                Elide = false,
            };

            var mat = new Matrix
            {
                Columns = 4,
                Units = 1,
            };

            string Formatter(Element e, byte[] d)
            {
                return "42";
            }

            var buff = new byte[] {42, 42, 42, 42, 42, 42, 42, 42};

            var result = mat.TryFormat(el, buff, Formatter);
            foreach (var row in result)
            {
                Assert.Equal("[ 42 42 42 42 ]", row);
            }
        }

        [Fact]
        public void TestFormatInts()
        {
            var el = new Element
            {
                Name = "Test",
                Units = 8,
                PrintFormat = Bender.PrintFormat.Ascii,
                Matrix = null,
                IsLittleEndian = true,
                IsSigned = true,
                Elide = false,
            };

            var mat = new Matrix
            {
                Columns = 1,
                Units = 4,
            };

            string Formatter(Element e, byte[] d)
            {
                return "0x42424242";
            }

            var buff = new byte[] { 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42 };

            var result = mat.TryFormat(el, buff, Formatter);
            foreach (var row in result)
            {
                Assert.Equal("[ 0x42424242 ]", row);
            }
        }

        [Fact]
        public void TestFormatEmpty()
        {
            var el = new Element
            {
                Name = "Test",
                Units = 8,
                PrintFormat = Bender.PrintFormat.Ascii,
                Matrix = null,
                IsLittleEndian = true,
                IsSigned = true,
                Elide = false,
            };

            var mat = new Matrix
            {
                Columns = 2,
                Units = 0,
            };

            Assert.Empty(mat.TryFormat(el, new byte[0], null));
        }

        [Fact]
        public void TestFormatTooShortOfBuffer()
        {
            var el = new Element
            {
                Name = "Test",
                Units = 8,
                PrintFormat = Bender.PrintFormat.Ascii,
                Matrix = null,
                IsLittleEndian = true,
                IsSigned = true,
                Elide = false,
            };

            var mat = new Matrix
            {
                Columns = 2,
                Units = 2,
            };

            string Formatter(Element e, byte[] d)
            {
                return "0x4242";
            }

            var buff = new byte[] { 0x42, 0x42 };

            var result = mat.TryFormat(el, buff, Formatter);
            foreach (var row in result)
            {
                Assert.Equal("[ 0x4242 ]", row);
            }
        }
    }
}