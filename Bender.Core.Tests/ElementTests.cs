using Xunit;

namespace Bender.Core.Tests
{
    public class ElementTests
    {
        [Fact]
        public void TestClone()
        {
            var src = new Element
            {
                Name = "Test",
                Units = 8,
                PrintFormat = Bender.PrintFormat.Ascii,
                Matrix = null,
                LittleEndian = true,
                IsSigned = true,
                Elide = false,
            };

            var clone = src.Clone();

            Assert.Equal(src.Name, clone.Name);
            Assert.Equal(src.Units, clone.Units);
            Assert.Equal(src.PrintFormat, clone.PrintFormat);
            Assert.Equal(src.Matrix, clone.Matrix);
            Assert.Equal(src.LittleEndian, clone.LittleEndian);
            Assert.Equal(src.IsSigned, clone.IsSigned);
            Assert.Equal(src.Elide, clone.Elide);
        }

        [Fact]
        public void TestToString()
        {
            var src = new Element
            {
                Name = "Test",
                Units = 8,
                PrintFormat = Bender.PrintFormat.Ascii,
                Matrix = null,
                LittleEndian = true,
                IsSigned = true,
                Elide = false,
            };

            var actual = src.ToString();
            Assert.Contains(src.Name, actual);
            Assert.Contains(src.Units.ToString(), actual);
            Assert.Contains(src.PrintFormat.ToString(), actual);
            Assert.Contains(src.LittleEndian.ToString(), actual);
            Assert.Contains(src.IsSigned.ToString(), actual);
            Assert.Contains(src.Elide.ToString(), actual);
        }

        [Fact]
        public void TestEnumerateLayout()
        {
            var src = new Element
            {
                Name = "Test",
                Units = 8,
                PrintFormat = Bender.PrintFormat.Ascii,
                Matrix = null,
                LittleEndian = true,
                IsSigned = true,
                Elide = false,
            };

            var actual = string.Join(",", src.EnumerateLayout());
            Assert.Contains(src.Name, actual);
            Assert.Contains(src.Units.ToString(), actual);
            Assert.Contains(src.PrintFormat.ToString(), actual);
            Assert.Contains(src.LittleEndian.ToString(), actual);
            Assert.Contains(src.IsSigned.ToString(), actual);
            Assert.Contains(src.Elide.ToString(), actual);
        }
    }
}
