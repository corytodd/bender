using System.IO;
using Xunit;

namespace BenderLib.Tests
{
    public class BenderPrinterTests
    {
        [Fact]
        public void TestPrint()
        {
            var spec = new SpecParser().Parse(DataFile.From(Properties.Resources.simple_layout));
            var bender = new BinaryParser(spec).Parse(DataFile.From(Properties.Resources.simple_layout_binary));
            
            var printer = new BenderPrinter();

            var stream = new MemoryStream();
            printer.WriteStream(bender, stream);

            Assert.True(stream.Length > 0);
            Assert.True(stream.Position == 0);
        }
    }
}
