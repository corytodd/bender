using Xunit;

namespace BenderLib.Tests
{
    public class MatrixTests
    {
        [Fact]
        public void TestFormatBytes()
        {
            var el = new Element
            {
                Name = "Test",
                Width = 8,
                Format = ElementFormat.ASCII,
                Matrix = string.Empty,
                LittleEndian = true,
                IsSigned = true,
                Elide = false,
                IsReadOnly = true,
            };

            var mat = new Matrix
            {
                Name = "Test",
                Columns = 4,
                Units = 1,
            };

            string Formatter(Element e, byte[] d)
            {
                return "42";
            }

            var buff = new byte[] {42, 42, 42, 42, 42, 42, 42, 42};

            var result = mat.Format(el, buff, Formatter);
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
                Width = 8,
                Format = ElementFormat.ASCII,
                Matrix = string.Empty,
                LittleEndian = true,
                IsSigned = true,
                Elide = false,
                IsReadOnly = true,
            };

            var mat = new Matrix
            {
                Name = "Test",
                Columns = 1,
                Units = 4,
            };

            string Formatter(Element e, byte[] d)
            {
                return "0x42424242";
            }

            var buff = new byte[] { 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42 };

            var result = mat.Format(el, buff, Formatter);
            foreach (var row in result)
            {
                Assert.Equal("[ 0x42424242 ]", row);
            }

            // make sure nothing stepped on the matrix name
            Assert.False(string.IsNullOrEmpty(mat.Name));
        }

        [Fact]
        public void TestFormatEmpty()
        {
            var el = new Element
            {
                Name = "Test",
                Width = 8,
                Format = ElementFormat.ASCII,
                Matrix = string.Empty,
                LittleEndian = true,
                IsSigned = true,
                Elide = false,
                IsReadOnly = true,
            };

            var mat = new Matrix
            {
                Name = "Test",
                Columns = 2,
                Units = 0,
            };

            Assert.Empty(mat.Format(el, new byte[0], null));
        }

        [Fact]
        public void TestFormatTooShortOfBuffer()
        {
            var el = new Element
            {
                Name = "Test",
                Width = 8,
                Format = ElementFormat.ASCII,
                Matrix = string.Empty,
                LittleEndian = true,
                IsSigned = true,
                Elide = false,
                IsReadOnly = true,
            };

            var mat = new Matrix
            {
                Name = "Test",
                Columns = 2,
                Units = 2,
            };

            string Formatter(Element e, byte[] d)
            {
                return "0x4242";
            }

            var buff = new byte[] { 0x42, 0x42 };

            var result = mat.Format(el, buff, Formatter);
            foreach (var row in result)
            {
                Assert.Equal("[ 0x4242 ]", row);
            }

            // make sure nothing stepped on the matrix name
            Assert.False(string.IsNullOrEmpty(mat.Name));
        }
    }
}