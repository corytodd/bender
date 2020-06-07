using Xunit;

namespace Bender.Core.Tests
{
    public class NumberTests
    {
        [Fact]
        public void TestSignedByte()
        {
            var el = new Element
            {
                PrintFormat = Bender.PrintFormat.Decimal,
                Units = 1,
                IsSigned = true,
            };


            // Test the widest data type because it includes all smaller data types
            Assert.Equal(0, new Number(el, new byte[] {0}).sl);
            Assert.True(127 == new Number(el, new byte[] {127}).sl);
            Assert.Equal(-1, new Number(el, new byte[] {255}).sl);
            Assert.Equal(-128, new Number(el, new byte[] {128}).sl);
        }

        [Fact]
        public void TestUnsignedByte()
        {
            var el = new Element
            {
                PrintFormat = Bender.PrintFormat.Decimal,
                Units = 1,
                IsSigned = false,
            };

            Assert.True(0 == new Number(el, new byte[] {0}).ul);
            Assert.True(127 == new Number(el, new byte[] {127}).ul);
            Assert.True(255 == new Number(el, new byte[] {255}).ul);
            Assert.True(128 == new Number(el, new byte[] {128}).ul);
        }

        [Fact]
        public void TestSignedShort()
        {
            var el = new Element
            {
                PrintFormat = Bender.PrintFormat.Decimal,
                Units = 2,
                IsSigned = true,
                IsLittleEndian = true
            };


            // Test the widest data type because it includes all smaller data types
            Assert.Equal(0, new Number(el, new byte[] {0, 0}).sl);
            Assert.Equal(32767, new Number(el, new byte[] {0xFF, 0x7F}).sl);
            Assert.Equal(-32768, new Number(el, new byte[] {0x00, 0x80}).sl);
            Assert.Equal(-1, new Number(el, new byte[] {0xFF, 0xFF}).sl);
        }

        [Fact]
        public void TestUnsignedShort()
        {
            var el = new Element
            {
                PrintFormat = Bender.PrintFormat.Decimal,
                Units = 2,
                IsSigned = false,
                IsLittleEndian = true
            };

            Assert.True(0 == new Number(el, new byte[] {0, 0}).ul);
            Assert.True(32767 == new Number(el, new byte[] {0xFF, 0x7F}).ul);
            Assert.True(32768 == new Number(el, new byte[] {0x00, 0x80}).ul);
            Assert.True(65535 == new Number(el, new byte[] {0xFF, 0xFF}).ul);
        }

        [Fact]
        public void TestSignedInt()
        {
            var el = new Element
            {
                PrintFormat = Bender.PrintFormat.Decimal,
                Units = 4,
                IsSigned = true,
                IsLittleEndian = true
            };


            // Test the widest data type because it includes all smaller data types
            Assert.Equal(0, new Number(el, new byte[] {0, 0, 0, 0}).sl);
            Assert.Equal(2147483647, new Number(el, new byte[] {0xFF, 0xFF, 0xFF, 0x7F}).sl);
            Assert.Equal(-2147483648, new Number(el, new byte[] {0x00, 0x00, 0x00, 0x80}).sl);
            Assert.Equal(-1, new Number(el, new byte[] {0xFF, 0xFF, 0xFF, 0xFF}).sl);
        }

        [Fact]
        public void TestUnsignedInt()
        {
            var el = new Element
            {
                PrintFormat = Bender.PrintFormat.Decimal,
                Units = 4,
                IsSigned = false,
                IsLittleEndian = true
            };

            Assert.True(0 == new Number(el, new byte[] {0, 0, 0, 0}).ul);
            Assert.True(2147483647 == new Number(el, new byte[] {0xFF, 0xFF, 0xFF, 0x7F}).ul);
            Assert.True(2147483648 == new Number(el, new byte[] {0x00, 0x00, 0x00, 0x80}).ul);
            Assert.True(4294967295 == new Number(el, new byte[] {0xFF, 0xFF, 0xFF, 0xFF}).ul);
        }

        [Fact]
        public void TestSignedLong()
        {
            var el = new Element
            {
                PrintFormat = Bender.PrintFormat.Decimal,
                Units = 8,
                IsSigned = true,
                IsLittleEndian = true
            };


            // Test the widest data type because it includes all smaller data types
            Assert.Equal(0, new Number(el, new byte[] {0, 0, 0, 0, 0, 0, 0, 0}).sl);
            Assert.Equal(9223372036854775807,
                new Number(el, new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F}).sl);
            Assert.Equal(-9223372036854775808,
                new Number(el, new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80}).sl);
            Assert.Equal(-1, new Number(el, new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF}).sl);
        }

        [Fact]
        public void TestUnsignedLong()
        {
            var el = new Element
            {
                PrintFormat = Bender.PrintFormat.Decimal,
                Units = 8,
                IsSigned = false,
                IsLittleEndian = true
            };

            Assert.True(0 == new Number(el, new byte[] {0, 0, 0, 0, 0, 0, 0, 0}).ul);
            Assert.True(9223372036854775807 ==
                        new Number(el, new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F}).ul);
            Assert.True(9223372036854775808 ==
                        new Number(el, new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80}).ul);
            Assert.True(18446744073709551615 ==
                        new Number(el, new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF}).ul);
        }

        [Fact]
        public void TestEqualityOperatorOrder()
        {
            var element = new Element {Units = 4};
            var number = new Number(element, new byte[] {0, 0, 0, 1});

            // Operator order matters, both must be implemented
            Assert.True(number == 1);
            Assert.True(1 == number);
        }

        [Fact]
        public void TestInequalityOperatorOrder()
        {
            var element = new Element {Units = 4, IsLittleEndian = true};
            var number = new Number(element, new byte[] {0, 0, 0, 1});

            // Operator order matters, both must be implemented
            Assert.True(number != 1);
            Assert.True(1 != number);
        }

        [Fact]
        public void TestFloatSingle()
        {
            // Setup
            var element = new Element {Units = 4, IsLittleEndian = true, PrintFormat = Bender.PrintFormat.Float};
            var bytes = new byte[] {0x4D, 0x93, 0x49, 0x41};

            // Execute
            var actual = new Number(element, bytes);

            // Test
            const float expected = 12.5985f;
            Assert.Equal(actual.fs, expected, 1);
        }

        [Fact]
        public void TestFloatDouble()
        {
            // Setup
            var element = new Element {Units = 8, IsLittleEndian = true, PrintFormat = Bender.PrintFormat.Float};
            var bytes = new byte[] {0xd0, 0x5f, 0xc5, 0xb7, 0x53, 0x07, 0xf8, 0x40};

            // Execute
            var actual = new Number(element, bytes);

            // Test
            const double expected = 98421.232365965148231;
            Assert.Equal(actual.fd, expected, 1);
        }
    }
}