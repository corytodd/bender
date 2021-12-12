// ReSharper disable MemberCanBePrivate.Global - This is a library type, all fields must be public

namespace Bender.Core
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Layouts;
    using Rendering;

    /// <summary>
    ///     Number wrapper holds any number type
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public record Number : IRenderable
    {
        /// <summary>
        ///     True if floating point
        /// </summary>
        [FieldOffset(13)]
        private readonly bool _isFloat;

        /// <summary>
        ///     True if signed
        /// </summary>
        [FieldOffset(12)]
        private readonly bool _isSigned;

        /// <summary>
        ///     How number format should be handled
        /// </summary>
        [FieldOffset(14)]
        private readonly Bender.PrintFormat _printFormat;

        /// <summary>
        ///     Count of bytes stored
        /// </summary>
        [FieldOffset(8)]
        private readonly int _width;

        /// <summary>
        ///     Double precision float
        /// </summary>
        [FieldOffset(0)]
        public readonly double fd;

        /// <summary>
        ///     Single precision float
        /// </summary>
        [FieldOffset(0)]
        public readonly float fs;

        /// <summary>
        ///     Signed byte
        /// </summary>
        [FieldOffset(0)]
        public readonly sbyte sb;

        /// <summary>
        ///     signed int
        /// </summary>
        [FieldOffset(0)]
        public readonly int si;

        /// <summary>
        ///     Signed long
        /// </summary>
        [FieldOffset(0)]
        public readonly long sl;

        /// <summary>
        ///     Signed short
        /// </summary>
        [FieldOffset(0)]
        public readonly short ss;

        /// <summary>
        ///     Unsigned byte
        /// </summary>
        [FieldOffset(0)]
        public readonly byte ub;

        /// <summary>
        ///     unsigned int
        /// </summary>
        [FieldOffset(0)]
        public readonly uint ui;

        /// <summary>
        ///     Unsigned long
        /// </summary>
        [FieldOffset(0)]
        public readonly ulong ul;

        /// <summary>
        ///     Unsigned short
        /// </summary>
        [FieldOffset(0)]
        public readonly ushort us;

        /// <summary>
        ///     Converts raw buffer data into a numeric type. This handles
        ///     sign conversion. Data is not checked for length validity, the caller
        ///     must take caution to ensure data has exactly the number of bytes for the
        ///     format prescribed by the element specification.
        /// </summary>
        /// <param name="el">Element spec</param>
        /// <param name="data">Raw data</param>
        /// <returns>Number type</returns>
        /// <exception cref="ArgumentException">Thrown is element width is not in {1,2,4,8}</exception>
        public Number(Element el, byte[] data) : this(el.Units, el.IsSigned, el.IsLittleEndian, el.PrintFormat, data)
        {
        }

        /// <summary>
        ///     Converts raw buffer data into a numeric type. This handles
        ///     sign conversion. Data is not checked for length validity, the caller
        ///     must take caution to ensure data has exactly the number of bytes for the
        ///     format prescribed by the element specification.
        /// </summary>
        /// <param name="units">Interpret data at this width</param>
        /// <param name="isSigned">True if value is signed</param>
        /// <param name="isLittleEndian">True if number should be interpretted little Endian</param>
        /// <param name="printFormat">Format type</param>
        /// <param name="data">Raw data</param>
        /// <returns>Number type</returns>
        /// <exception cref="ArgumentException">Thrown is element width is not in {1,2,4,8}</exception>
        public Number(int units, bool isSigned, bool isLittleEndian, Bender.PrintFormat printFormat, byte[] data)
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
            if (!(isLittleEndian && BitConverter.IsLittleEndian) ||
                (printFormat == Bender.PrintFormat.BigInt && isLittleEndian))
            {
                Array.Reverse(data);
            }

            const int offset = 0;
            _width = units;
            _isSigned = isSigned;
            _isFloat = printFormat == Bender.PrintFormat.Float;
            _printFormat = printFormat;

            // Set the long number for everything so any field can be 
            // access correctly
            switch (_width)
            {
                case 1:
                    if (_isSigned)
                    {
                        sl = (sbyte)data[0];
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

        /// <inheritdoc />
        public string Format()
        {
            switch (_printFormat)
            {
                case Bender.PrintFormat.Binary:
                    return Convert.ToString(si, 2).PadLeft(_width * 8, '0');
                case Bender.PrintFormat.Octal:
                    return $"O{Convert.ToString(sl, 8)}";
                case Bender.PrintFormat.Decimal:
                    return sl.ToString();
                case Bender.PrintFormat.Hex:
                case Bender.PrintFormat.BigInt:
                    var width = (_width * 2).NextPowerOf2();
                    var hex = Convert.ToString(sl, 16).PadLeft(width, '0').ToUpper();
                    return $"0x{hex}";

                case Bender.PrintFormat.Float:
                    var result = _width switch
                    {
                        // Reinterpret data as floating point
                        4 => fs.ToString("F6"),
                        8 => fd.ToString("F6"),
                        _ => "Malformed float. Width must be 4 or 8 bytes"
                    };
                    return result;

                case Bender.PrintFormat.Ascii:
                case Bender.PrintFormat.Unicode:
                    throw new ParseException("Cannot format numbers as a string type. This is bug");

                default:
                    return $"Unsupported format: {_printFormat}";
            }
        }

        /// <inheritdoc />
        public void Render(StreamWriter stream)
        {
            stream.Write(Format());
        }

        /// <summary>
        ///     Returns true if values are equal
        /// </summary>
        public bool Equals(int other)
        {
            return sl == other;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return si;
        }

        /// <summary>
        ///     Returns true if values are equal
        /// </summary>
        public static bool operator ==(Number left, int right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Returns true if values are equal
        /// </summary>
        public static bool operator ==(int left, Number right)
        {
            return right.Equals(left);
        }

        /// <summary>
        ///     Returns true if values are not equal
        /// </summary>
        public static bool operator !=(Number left, int right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        ///     Returns true if values are not equal
        /// </summary>
        public static bool operator !=(int left, Number right)
        {
            return !right.Equals(left);
        }

        /// <summary>
        ///     Performs a signed compare
        /// </summary>
        public static bool operator <(Number left, int right)
        {
            return left.si < right;
        }

        /// <summary>
        ///     Performs a signed compare
        /// </summary>
        public static bool operator >(Number left, int right)
        {
            return left.si > right;
        }


        /// <summary>
        ///     Performs a signed compare
        /// </summary>
        public static bool operator <(int left, Number right)
        {
            return left < right.si;
        }

        /// <summary>
        ///     Performs a signed compare
        /// </summary>
        public static bool operator >(int left, Number right)
        {
            return left > right.si;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Format();
        }
    }
}