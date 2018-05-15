namespace BenderLib
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Number wrapper holds any number type
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Number
    {
        [FieldOffset(0)] public byte ub;

        [FieldOffset(0)] public sbyte sb;

        [FieldOffset(0)] public ushort us;

        [FieldOffset(0)] public short ss;

        [FieldOffset(0)] public uint ui;

        [FieldOffset(0)] public int si;

        [FieldOffset(0)] public ulong ul;

        [FieldOffset(0)] public long sl;

        /// <summary>
        /// Converts raw buffer data into a numeric type. This handles 
        /// sign conversion. Data is not checked for length validity, the caller
        /// must take caution to ensure data has exactly the number of bytes for the
        /// format perscribed by the element specification.
        /// </summary>
        /// <param name="el">Element spec</param>
        /// <param name="data">Raw data</param>
        /// <returns>Number type</returns>
        public static Number From(Element el, byte[] data)
        {
            var number = new Number();

            // Set the long number for everything so any field can be 
            // access correctly
            switch (el.Width)
            {
                case 1:
                    if (el.IsSigned) number.sl = (sbyte) data[0];
                    else number.ul = data[0];
                    break;
                case 2:
                    if (el.IsSigned) number.sl = BitConverter.ToInt16(data, 0);
                    else number.ul = BitConverter.ToUInt16(data, 0);
                    break;
                case 4:
                    if (el.IsSigned) number.sl = BitConverter.ToInt32(data, 0);
                    else number.ul = BitConverter.ToUInt32(data, 0);
                    break;
                case 8:
                    if (el.IsSigned) number.sl = BitConverter.ToInt64(data, 0);
                    else number.ul = BitConverter.ToUInt64(data, 0);
                    break;
            }

            return number;
        }
    }
}
