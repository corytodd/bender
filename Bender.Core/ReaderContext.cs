namespace Bender.Core
{
    using System;
    using System.IO;
    using Logging;

    /// <summary>
    ///     Reader context manages access to a binary reader
    /// </summary>
    public class ReaderContext : IDisposable
    {
        private static readonly ILog ReaderLog = LogProvider.GetLogger("ReaderLog");

        /// <summary>
        ///     Optional memory stream backing
        /// </summary>
        private readonly MemoryStream _memory;

        /// <summary>
        ///     Data source
        /// </summary>
        private readonly BinaryReader _reader;

        /// <summary>
        ///     Create a new reader context
        /// </summary>
        /// <param name="data">Data source</param>
        public ReaderContext(byte[] data)
        {
            Ensure.IsNotNull(nameof(data), data);
            _memory = new MemoryStream(data);
            _reader = new BinaryReader(_memory);
        }

        /// <summary>
        ///     Create a new reader context
        /// </summary>
        /// <param name="reader">Data source</param>
        /// <exception cref="ArgumentException">Thrown if reader cannot be read or is unseekable</exception>
        public ReaderContext(BinaryReader reader)
        {
            Ensure.IsNotNull(nameof(reader), reader);
            Ensure.IsValid(nameof(reader), reader.BaseStream.CanRead);
            Ensure.IsValid(nameof(reader), reader.BaseStream.CanSeek);

            _reader = reader;
        }

        /// <summary>
        ///     Get or set reader position
        /// </summary>
        public long Position
        {
            get => _reader.BaseStream.Position;
            private set => _reader.BaseStream.Seek(value, SeekOrigin.Begin);
        }

        /// <summary>
        ///     Size in bytes of underlying data source
        /// </summary>
        public long Length => _reader.BaseStream.Length;

        /// <summary>
        ///     Read and return count bytes from current position
        /// </summary>
        /// <param name="count">Count of bytes to read</param>
        /// <returns>Read data</returns>
        public byte[] ReadBytes(int count)
        {
            return _reader.ReadBytes(count);
        }

        /// <summary>
        ///     Read and return <see cref="BinaryPointer"/> from current position
        /// </summary>
        /// <returns>BinaryPointer</returns>
        public BinaryPointer ReadBinaryPointer()
        {
            var size = _reader.ReadUInt32();
            var offset = _reader.ReadUInt32();

            return new BinaryPointer(size, offset);
        }

        /// <summary>
        ///     From the current stream position, read
        ///     in the next binary pointer type. The reader
        ///     position will be left 8 bytes ahead of the current
        ///     position.
        /// </summary>
        /// <returns>Deferred data</returns>
        public byte[] DeferredRead(BinaryPointer? pointer = null)
        {
            var readPointer = pointer ?? ReadBinaryPointer();

            // No data to read, must be a placeholder
            if (readPointer.SizeBytes == 0)
            {
                return new byte[0];
            }

            ReaderLog.Debug("[deferred.abs]{0}/0x{1:X4}", readPointer, Length);

            // We must leave the read position immediately after the offset read position
            var bookmark = Position;
            Position = readPointer.Offset;

            var result = ReadBytes((int) readPointer.SizeBytes);

            // Return to end of binary pointer
            Position = bookmark;

            return result;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _reader.Dispose();
            _memory?.Dispose();
        }
    }
}