using System.IO;
using Xunit;

namespace Bender.Core.Tests
{
    public class BenderPrinterTests
    {
        /// <summary>
        /// Test that a known specification produces a byte-for-byte ouput
        /// </summary>
        [Fact]
        public void TestPrint()
        {
            // Setup
            var spec = SpecFile.Parse(DataFile.From(Properties.Resources.simple_layout));
            var parser=  new BinaryParser(spec);
            var dataFile = DataFile.From(Properties.Resources.simple_layout_binary);
            var bender = parser.Parse(dataFile);
            
            var captureStream = new MemoryStream();
            var binaryWriter = new StreamWriter(captureStream);
            
            // Execute
            var printer = new BenderPrinter();
            printer.WriteStream(bender, binaryWriter);

            // Test
            Assert.True(captureStream.Length > 0);
            Assert.True(captureStream.Position == 0);

            var actual = System.Text.Encoding.UTF8.GetString(captureStream.ToArray());
            var expected = @"Element              Value           
================================================================================
Magic Number         abcd            
Version              1               
Counter              -1              
Flags                b00000010       
CRC16                0x0103          
Payload              Elided 16 bytes 
Transform A          [ 5 4 0 1 ]     
                     [ 2 3 4 5 ]     
                     [ 6 7 8 9 ]     
                     [ 10 11 12 13 ] 
Transform B          [ 3854 272 ]    
                     [ 770 1284 ]    
                     [ 1798 2312 ]   
                     [ 2826 3340 ]   
Range                [ Pair Count: 17829646 ]
                     [ Start: 0x05040302 ]
                     [ End: 0x09080706 ]
";
            
            Assert.Equal(expected, actual);
        }
    }
}
