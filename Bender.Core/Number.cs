namespace Bender.Core
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Number wrapper holds any number type
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Number
    {
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
        public byte ub;

        /// <summary>
        /// Signed byte
        /// </summary>
        [FieldOffset(0)]
        public sbyte sb;

        /// <summary>
        /// Unsigned short
        /// </summary>
        [FieldOffset(0)]
        public ushort us;

        /// <summary>
        /// Signed short
        /// </summary>
        [FieldOffset(0)]
        public short ss;

        /// <summary>
        /// unsigned int
        /// </summary>
        [FieldOffset(0)]
        public uint ui;

        /// <summary>
        /// signed int
        /// </summary>
        [FieldOffset(0)]
        public int si;

        /// <summary>
        /// Unsigned long
        /// </summary>
        [FieldOffset(0)]
        public ulong ul;

        /// <summary>
        /// Signed long
        /// </summary>
        [FieldOffset(0)]
        public long sl;

        /// <summary>
        /// Single precision float
        /// </summary>
        [FieldOffset(0)]
        public float fs;

        /// <summary>
        /// Double precision float
        /// </summary>
        [FieldOffset(0)]
        public double fd;

        /// <summary>
        /// Converts raw buffer data into a numeric type. This handles 
        /// sign conversion. Data is not checked for length validity, the caller
        /// must take caution to ensure data has exactly the number of bytes for the
        /// format prescribed by the element specification.
        /// </summary>
        /// <param name="el">Element spec</param>
        /// <param name="data">Raw data</param>
        /// <returns>Number type</returns>
        public static Number From(Element el, byte[] data)
        {
            const int offset = 0;
            var width = el.Units;
            var isSigned = el.IsSigned;
            var isFloat = el.PrintFormat == Bender.PrintFormat.Float;
            var number = new Number();

            // Set the long number for everything so any field can be 
            // access correctly
            switch (width)
            {
                case 1:
                    if (isSigned)
                    {
                        number.sl = (sbyte) data[0];
                    }
                    else
                    {
                        number.ul = data[0];
                    }

                    break;
                case 2:
                    if (isSigned)
                    {
                        number.sl = BitConverter.ToInt16(data, offset);
                    }
                    else
                    {
                        number.ul = BitConverter.ToUInt16(data, offset);
                    }

                    break;
                case 4:
                    if (isFloat)
                    {
                        number.fs = BitConverter.ToSingle(data);
                    }
                    else if (isSigned)
                    {
                        number.sl = BitConverter.ToInt32(data, offset);
                    }
                    else
                    {
                        number.ul = BitConverter.ToUInt32(data, offset);
                    }

                    break;
                case 8:
                    if (isFloat)
                    {
                        number.fd = BitConverter.ToDouble(data);
                    }
                    else if (isSigned)
                    {
                        number.sl = BitConverter.ToInt64(data, offset);
                    }
                    else
                    {
                        number.ul = BitConverter.ToUInt64(data, offset);
                    }

                    break;
            }

            return number;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return sl.ToString();
        }
    }
}