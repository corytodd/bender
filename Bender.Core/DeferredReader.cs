namespace Bender.Core
{
    using System;
    using System.IO;
    using Logging;

    /// <summary>
    ///     Extracts a deferred chunk of data
    ///     from a stream using the concept of
    ///     a binary pointer. A binary pointer is
    ///     a size and offset, each of which are
    ///     4 bytes in size and stored Big Endian.
    /// </summary>
    public class DeferredReader
    {
        private static readonly ILog ReaderLog = LogProvider.GetLogger("ReaderLog");
        
        /// <summary>
        ///     Do not dispose of this stream
        /// </summary>
        private readonly BinaryReader _reader;

        /// <summary>
        ///     Create a new deferred reader
        /// </summary>
        /// <param name="reader">Data source</param>
        /// <exception cref="ArgumentException">Thrown if reader cannot be read or is unseekable</exception>
        public DeferredReader(BinaryReader reader)
        {
            Ensure.IsNotNull(nameof(reader), reader);
            Ensure.IsValid(nameof(reader), reader.BaseStream.CanRead);
            Ensure.IsValid(nameof(reader), reader.BaseStream.CanSeek);

            _reader = reader;
        }

        /// <summary>
        ///     From the current stream position, read
        ///     in the next binary pointer type. The reader
        ///     position will be left 8 bytes ahead of the current
        ///     position.
        /// </summary>
        /// <returns>Deferred data</returns>
        public byte[] Read()
        {
            var size = _reader.ReadInt32();
            Ensure.IsValid(nameof(size), size >= 0);

            var offset = _reader.ReadInt32();
            Ensure.IsValid(nameof(offset), offset >= 0);

            // No data to read, must be a placeholder
            if (size == 0)
            {
                return new byte[0];
            }

            ReaderLog.Debug("[deferred.abs]{0,4}@0x{1:X4}/0x{2:X4}", size, offset,
                _reader.BaseStream.Length);

            // We must leave the read position immediately after the offset read position
            var bookmark = _reader.BaseStream.Position;
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            
            var result = _reader.ReadBytes((int) size);

            _reader.BaseStream.Position = bookmark;

            return result;
        }
    }
}