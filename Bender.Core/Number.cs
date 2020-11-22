// ReSharper disable MemberCanBePrivate.Global - This is a library type, all fields must be public

namespace Bender.Core
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Layouts;
    using Rendering;

    /// <summary>
    /// Number wrapper holds any number type
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Number : IRenderable
    {
        private readonly int _width;
        private readonly bool _isSigned;
        private readonly bool _isFloat;

        /// <summary>
        /// Converts raw buffer data into a numeric type. This handles 
        /// sign conversion. Data is not checked for length validity, the caller
        /// must take caution to ensure data has exactly the number of bytes for the
        /// format prescribed by the element specification.
        /// </summary>
        /// <param name="el">Element spec</param>
        /// <param name="data">Raw data</param>
        /// <returns>Number type</returns>
        /// <exception cref="ArgumentException">Thrown is element width is not in {1,2,4,8}</exception>
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
            _width = el.Units;
            _isSigned = el.IsSigned;
            _isFloat = el.PrintFormat == Bender.PrintFormat.Float;

            // Set the long number for everything so any field can be 
            // access correctly
            switch (_width)
            {
                case 1:
                    if (_isSigned)
                    {
                        sl = (sbyte) data[0];
                    }
                    else
                    {
                        ul = data[0];
                    }

                    break;
                case 2:
                    if (_isSigned)
                    {
                        sl = BitConverter.ToInt16(data, offset);
                    }
                    else
                    {
                        ul = BitConverter.ToUInt16(data, offset);
                    }

                    break;
                case 4:
                    if (_isFloat)
                    {
                        fs = BitConverter.ToSingle(data);
                    }
                    else if (_isSigned)
                    {
                        sl = BitConverter.ToInt32(data, offset);
                    }
                    else
                    {
                        ul = BitConverter.ToUInt32(data, offset);
                    }

                    break;
                case 8:
                    if (_isFloat)
                    {
                        fd = BitConverter.ToDouble(data);
                    }
                    else if (_isSigned)
                    {
                        sl = BitConverter.ToInt64(data, offset);
                    }
                    else
                    {
                        ul = BitConverter.ToUInt64(data, offset);
                    }

                    break;

                default:
                    throw new ArgumentException($"Unsupported width: {_width}");
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

        public string Format(string format)
        {
            switch (_width)
            {
                case 1:
                    return string.Format(format, _isSigned ? sb : ub);
                case 2:
                    return string.Format(format, _isSigned ? ss : us);
                case 4:
                    return string.Format(format,
                        _isFloat ? fs
                        : _isSigned ? si : ui
                    );
                case 8:
                    return string.Format(format,
                        _isFloat ? fd
                        : _isSigned ? sl : ul
                    );

                default:
                    return string.Empty;
            }
        }

        public void Print(StreamWriter stream)
        {
            throw new NotImplementedException();
        }
    }
}