using System.Collections.Generic;
using Xunit;

namespace BenderLib.Tests
{
    public class ExtensionsTests
    {
        [Fact]
        public void TestAsChunksEvenSize()
        {
            var data = new byte[] {0, 1, 0, 1, 0, 1};
            var expected = new List<byte[]>
            {
                new byte[] {0, 1},
                new byte[] {0, 1},
                new byte[] {0, 1},
            };

            Assert.Equal(expected, data.AsChunks(2));
        }

        [Fact]
        public void TestAsChunksOddSize()
        {
            var data = new byte[] { 0, 1, 0, 1, 0, 1, 0 };
            var expected = new List<byte[]>
            {
                new byte[] {0, 1},
                new byte[] {0, 1},
                new byte[] {0, 1},
                new byte[] { 0 },
            };

            Assert.Equal(expected, data.AsChunks(2));
        }

        [Fact]
        public void TestEmpty()
        {
            var data = new byte[0];

            Assert.Equal(new List<byte[]>(), data.AsChunks(2));
        }

        [Fact]
        public void TestTooLong()
        {
            var data = new byte[] { 0, 1, 0, 1, 0, 1, 0 };
            var expected = new List<byte[]>
            {
                new byte[]  { 0, 1, 0, 1, 0, 1, 0 }
            };

            Assert.Equal(expected, data.AsChunks(100));
        }
    }
}
