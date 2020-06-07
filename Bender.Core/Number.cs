// ReSharper disable MemberCanBePrivate.Global - This is a library type, all fields must be public
namespace Bender.Core
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Number wrapper holds any number type
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Number
    {
        /// <summary>
        /// Converts raw buffer data into a numeric type. This handles 
        /// sign conversion. Data is not checked for length validity, the caller
        /// must take caution to ensure data has exactly the number of bytes for the
        /// format prescribed by the element specification.
        /// </summary>
        /// <param name="el">Element spec</param>
        /// <param name="data">Raw data</param>
        /// <returns>Number type</returns>
        public Number(Element el, byte[] data)
        {
            // Default values required for readonly struct
            ub = 0;
            sb = 0;
            us = 0;
            ss = 0;
            ui = 0;
            si = 0;
            ul = 0;
            sl = 0;
            fs = 0;
            fd = 0;
            
            // If byte order does not match, flip now
            // Or if this is a bigint and the source is little endian, flip now
            if (!(el.IsLittleEndian && BitConverter.IsLittleEndian) ||
                (el.PrintFormat == Bender.PrintFormat.BigInt && el.IsLittleEndian))
            {
                Array.Reverse(data);
            }

            const int offset = 0;
            var width = el.Units;
            var isSigned = el.IsSigned;
            var isFloat = el.PrintFormat == Bender.PrintFormat.Float;

            // Set the long number for everything so any field can be 
            // access correctly
            switch (width)
            {
                case 1:
                    if (isSigned)
                    {
                        sl = (sbyte) data[0];
                    }
                    else
                    {
                        ul = data[0];
                    }

                    break;
                case 2:
                    if (isSigned)
                    {
                        sl = BitConverter.ToInt16(data, offset);
                    }
                    else
                    {
                        ul = BitConverter.ToUInt16(data, offset);
                    }

                    break;
                case 4:
                    if (isFloat)
                    {
                        fs = BitConverter.ToSingle(data);
                    }
                    else if (isSigned)
                    {
                        sl = BitConverter.ToInt32(data, offset);
                    }
                    else
                    {
                        ul = BitConverter.ToUInt32(data, offset);
                    }

                    break;
                case 8:
                    if (isFloat)
                    {
                        fd = BitConverter.ToDouble(data);
                    }
                    else if (isSigned)
                    {
                        sl = BitConverter.ToInt64(data, offset);
                    }
                    else
                    {
                        ul = BitConverter.ToUInt64(data, offset);
                    }

                    break;
            }
        }

        /// <summary>
        /// Returns true if values are equal
        /// </summary>
        public bool Equals(int other)
        {
            return sl == other;
        }

        /// <summary>
        /// Returns true if values are equal
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is int other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return si;
        }

        /// <summary>
        /// Returns true if values are equal
        /// </summary>
        public static bool operator ==(Number left, int right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns true if values are equal
        /// </summary>
        public static bool operator ==(int left, Number right)
        {
            return right.Equals(left);
        }

        /// <summary>
        /// Returns true if values are not equal
        /// </summary>
        public static bool operator !=(Number left, int right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns true if values are not equal
        /// </summary>
        public static bool operator !=(int left, Number right)
        {
            return !right.Equals(left);
        }

        /// <summary>
        /// Performs a signed compare
        /// </summary>
        public static bool operator <(Number left, int right)
        {
            return left.si < right;
        }

        /// <summary>
        /// Performs a signed compare
        /// </summary>
        public static bool operator >(Number left, int right)
        {
            return left.si > right;
        }


        /// <summary>
        /// Performs a signed compare
        /// </summary>
        public static bool operator <(int left, Number right)
        {
            return left < right.si;
        }

        /// <summary>
        /// Performs a signed compare
        /// </summary>
        public static bool operator >(int left, Number right)
        {
            return left > right.si;
        }

        /// <summary>
        /// Unsigned byte
        /// </summary>
        [FieldOffset(0)]
        public readonly byte ub;

        /// <summary>
        /// Signed byte
        /// </summary>
        [FieldOffset(0)]
        public readonly sbyte sb;

        /// <summary>
        /// Unsigned short
        /// </summary>
        [FieldOffset(0)]
        public readonly ushort us;

        /// <summary>
        /// Signed short
        /// </summary>
        [FieldOffset(0)]
        public readonly short ss;

        /// <summary>
        /// unsigned int
        /// </summary>
        [FieldOffset(0)]
        public readonly uint ui;

        /// <summary>
        /// signed int
        /// </summary>
        [FieldOffset(0)]
        public readonly int si;

        /// <summary>
        /// Unsigned long
        /// </summary>
        [FieldOffset(0)]
        public readonly ulong ul;

        /// <summary>
        /// Signed long
        /// </summary>
        [FieldOffset(0)]
        public readonly long sl;

        /// <summary>
        /// Single precision float
        /// </summary>
        [FieldOffset(0)]
        public readonly float fs;

        /// <summary>
        /// Double precision float
        /// </summary>
        [FieldOffset(0)]
        public readonly double fd;

        /// <inheritdoc />
        public override string ToString()
        {
            return sl.ToString();
        }
    }
}