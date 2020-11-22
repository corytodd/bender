namespace Bender.Core.Tests
{
    using System.Collections.Generic;
    using Layouts;
    using Xunit;

    public class EnumerationTests
    {
        private static readonly Enumeration _enumeration = new Enumeration
        {
            Name = "Test Enumeration",
            Values = new Dictionary<int, string>
            {
                {0, "None"},
                {1, "First"},
                {2, "Second"},
            }
        };

        private static readonly Element _element = new Element
        {
            Enumeration = new Enumeration
            {
                Name = "Test Enumeration",
                Values = new Dictionary<int, string>
                {
                    {0, "Foo"}
                }
            },
            Name = "Element w/ Enumeration",
            Units = 1
        };

        [Theory]
        [InlineData(new byte[] {0}, "None")]
        [InlineData(new byte[] {1}, "First")]
        [InlineData(new byte[] {2}, "Second")]
        public void CanFormatElement(byte[] buff, string expected)
        {
            var actual = _element.BuildNode(buff);
            Assert.Equal(expected, actual.Name);
        }
    }
}