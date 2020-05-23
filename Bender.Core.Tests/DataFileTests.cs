using System;
using System.IO;
using Xunit;

namespace Bender.Core.Tests
{
    public class DataFileTests : IDisposable
    {
        private readonly string _mTempdir;

        public DataFileTests()
        {
            var root = AppDomain.CurrentDomain.BaseDirectory;
            _mTempdir = Path.GetFullPath(Path.Combine(root, "test_temp"));

            if (Directory.Exists(_mTempdir))
            {
                Directory.Delete(_mTempdir, true);
            }

            Directory.CreateDirectory(_mTempdir);
        }

        [Fact]
        public void TestFromBytes()
        {
            var data = Properties.Resources.simple_layout;
            var df = DataFile.From(data);

            Assert.False(df.Empty);
            Assert.Equal(df.Size, data.Length);
            Assert.Equal(data, df.Data);
        }

        [Fact]
        public void TestToString()
        {
            const string expected = "Hello World";
            var data = System.Text.Encoding.ASCII.GetBytes(expected);

            var df = DataFile.From(data);
            Assert.Equal(expected, df.ToString());

            var reader = df.AsStringReader();
            Assert.Equal(expected, reader.ReadToEnd());
        }

        [Fact]
        public void TestExistingFile()
        {
            var data = Properties.Resources.simple_layout;
            var path = Path.Combine(_mTempdir, "test.file");
            File.WriteAllBytes(path, data);

            var df = DataFile.From(path);

            Assert.False(df.Empty);
            Assert.Equal(df.Size, data.Length);
            Assert.Equal(data, df.Data);
        }

        [Fact]
        public void TestNonExistingFile()
        {
            var df = DataFile.From("i_do_not_exist.[");

            Assert.True(df.Empty);
            Assert.Equal(0, df.Size);
        }

        public void Dispose()
        {
            if (Directory.Exists(_mTempdir))
            {
                Directory.Delete(_mTempdir, true);
            }
        }
    }
}