namespace Bender.Core
{
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Wraps arbitrary data
    /// </summary>
    [DebuggerDisplay("Size = {" + nameof(Size) + "}")]
    public class DataFile
    {
        private readonly byte[] _data;

        private DataFile(byte[] data)
        {
            _data = data;
        }

        /// <summary>
        /// Wraps data in a DataFile
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataFile From(byte[] data)
        {
            return new DataFile(data);
        }

        /// <summary>
        /// Reads bytes from filePath and returns new DataFile. If file read
        /// fails, and empty DataFile will be returned
        /// </summary>
        /// <param name="filePath">Path to file to read</param>
        /// <returns>Datafile</returns>
        public static DataFile From(string filePath)
        {
            return !File.Exists(filePath) ? From(new byte[0]) : new DataFile(File.ReadAllBytes(filePath));
        }

        /// <summary>
        /// Create a datafile directly from an ASCII string
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static DataFile FromASCII(string content)
        {
            var buff = System.Text.Encoding.ASCII.GetBytes(content);
            return new DataFile(buff);
        }

        /// <summary>
        /// Returns raw data as a UTF8 string
        /// </summary>
        /// <returns></returns>
        public override string ToString() => System.Text.Encoding.UTF8.GetString(_data);

        /// <summary>
        /// Wraps raw data in a string reader
        /// </summary>
        /// <returns></returns>
        public StringReader AsStringReader()
        {
            return new StringReader(ToString());
        }

        /// <summary>
        /// Returns a copy of underlying data
        /// </summary>
        public byte[] Data => (byte[])_data.Clone();

        /// <summary>
        /// Returns true if this DataFile is empty
        /// </summary>
        public bool Empty => _data.Length == 0;

        /// <summary>
        /// Returns length of data in bytes
        /// </summary>
        /// <returns></returns>
        public int Size => _data.Length;
    }
}
