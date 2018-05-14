namespace BenderLib
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Number wrapper holds any number type
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    struct Number
    {
        [FieldOffset(0)]
        public byte ub;

        [FieldOffset(0)]
        public sbyte sb;

        [FieldOffset(0)]
        public UInt16 us;

        [FieldOffset(0)]
        public Int16 ss;

        [FieldOffset(0)]
        public UInt32 ui;

        [FieldOffset(0)]
        public Int32 si;

        [FieldOffset(0)]
        public UInt64 ul;

        [FieldOffset(0)]
        public Int64 sl;

        [FieldOffset(0)]
        public double d;

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

            if (el.Format == ElementFormat.Float)
            {
                number.d = BitConverter.ToDouble(data, 0);
            }
            else
            {
                switch (el.Width)
                {
                    case 1:
                        if (el.IsSigned) number.sb = (sbyte)data[0];
                        else number.ub = data[0];
                        break;
                    case 2:
                        if (el.IsSigned) number.ss = BitConverter.ToInt16(data, 0);
                        else number.us = BitConverter.ToUInt16(data, 0);
                        break;
                    case 4:
                        if (el.IsSigned) number.si = BitConverter.ToInt32(data, 0);
                        else number.ui = BitConverter.ToUInt32(data, 0);
                        break;
                    case 8:
                        if (el.IsSigned) number.sl = BitConverter.ToInt64(data, 0);
                        else number.ul = BitConverter.ToUInt64(data, 0);
                        break;
                    default:
                        break;
                }
            }

            return number;
        }
    }
}
