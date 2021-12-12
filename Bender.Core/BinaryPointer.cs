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
        /// <param name="offset">Offset relative to start</param>
        /// <param name="size">Size in bytes</param>
        public BinaryPointer(uint offset, uint size)
        {
            Offset = offset;
            SizeBytes = size;
        }
        
        /// <summary>
        ///     Location of object relative to start of file
        /// </summary>
        [FieldOffset(0)]
        public readonly uint Offset;

        /// <summary>
        ///     Size of object in bytes
        /// </summary>
        [FieldOffset(4)]
        public readonly uint SizeBytes;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{SizeBytes} bytes @0x{Offset:X4}";
        }
    }
}