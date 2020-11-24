namespace Bender.Core
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     A pointer embedded into a binary
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct BinaryPointer
    {
        /// <summary>
        ///     Create a new binary pointer
        /// </summary>
        /// <param name="size">Size in bytes</param>
        /// <param name="offset">Offset relative to start</param>
        public BinaryPointer(uint size, uint offset)
        {
            SizeBytes = size;
            Offset = offset;
        }
        
        /// <summary>
        ///     Size of object in bytes
        /// </summary>
        [FieldOffset(0)]
        public readonly uint SizeBytes;

        /// <summary>
        ///     Location of object relative to start of file
        /// </summary>
        [FieldOffset(4)]
        public readonly uint Offset;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{SizeBytes} bytes @0x{Offset:X4}";
        }
    }
}