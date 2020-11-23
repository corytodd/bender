namespace Bender.Core
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;

    /// <summary>
    ///     Library extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Splits <paramref name="source"/> into chunks of size not greater than <paramref name="chunkMaxSize"/>
        /// </summary>
        /// <param name="source">Array to be split</param>
        /// <param name="chunkMaxSize">Max size of chunk</param>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="Array"/> of <typeparam name="T"/></returns>
        /// <see href="https://stackoverflow.com/q/47683209/1755158"/>
        public static IEnumerable<T[]> AsChunks<T>(this T[] source, int chunkMaxSize)
        {
            var pos = 0;
            var sourceLength = source.Length;
            do
            {
                var len = Math.Min(pos + chunkMaxSize, sourceLength) - pos;
                if (len == 0)
                {
                    yield break;
                }

                var arr = new T[len];
                Array.Copy(source, pos, arr, 0, len);
                pos += len;
                yield return arr;
            } while (pos < sourceLength);
        }

        /// <summary>
        /// Returns the nearest power of 2 greater than v
        /// For example 1->2, 5->8, 13->16
        /// </summary>
        /// <param name="v">Starting value</param>
        /// <returns>Next greatest power of 2</returns>
        public static int NextPowerOf2(this int v)
        {
            if (v == 1)
            {
                return 2;
            }

            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="isLittleEndian"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] As<T>(this byte[] arr, bool isLittleEndian)
        {
            T[] result = null;

            if (typeof(T) == typeof(byte))
            {
                result = new T[arr.Length];
                Array.Copy(arr, result, arr.Length);
            }
            else if (typeof(T) == typeof(long))
            {
                if (arr.Length % 8 != 0)
                {
                    throw new ArgumentException("Array is not aligned to 4 bytes");
                }

                var temp = new long[arr.Length / 8];
                for (var index = 0; index < temp.Length; ++index)
                {
                    var val = BitConverter.ToInt64(arr, index);
                    val = isLittleEndian && !BitConverter.IsLittleEndian
                        ? BinaryPrimitives.ReverseEndianness(val)
                        : val;
                    temp[index] = val;
                }

                result = new T[temp.Length];
                Array.Copy(temp, result, temp.Length);
            }
            else if (typeof(T) == typeof(int))
            {
                if (arr.Length % 4 != 0)
                {
                    throw new ArgumentException("Array is not aligned to 4 bytes");
                }

                var temp = new int[arr.Length / 4];
                for (var index = 0; index < temp.Length; ++index)
                {
                    var val = BitConverter.ToInt32(arr, index);
                    val = isLittleEndian && !BitConverter.IsLittleEndian
                        ? BinaryPrimitives.ReverseEndianness(val)
                        : val;
                    temp[index] = val;
                }

                result = new T[temp.Length];
                Array.Copy(temp, result, temp.Length);
            }
            else if (typeof(T) == typeof(short))
            {
                if (arr.Length % 2 != 0)
                {
                    throw new ArgumentException("Array is not aligned to 4 bytes");
                }

                var temp = new short[arr.Length / 2];
                for (var index = 0; index < temp.Length; ++index)
                {
                    var val = BitConverter.ToInt16(arr, index);
                    val = isLittleEndian && !BitConverter.IsLittleEndian
                        ? BinaryPrimitives.ReverseEndianness(val)
                        : val;
                    temp[index] = val;
                }

                result = new T[temp.Length];
                Array.Copy(temp, result, temp.Length);
            }

            return result;
        }

        /// <summary>
        ///     Reshape flat array into multi dimensional array
        /// </summary>
        /// <param name="arr">Source data</param>
        /// <param name="rows">Count of rows in output</param>
        /// <param name="cols">Count of columns in output</param>
        /// <typeparam name="T">Array type</typeparam>
        /// <returns>Multi dimensional array</returns>
        public static T[,] Reshape<T>(this T[] arr, int rows, int cols)
        {
            var result = new T[rows, cols];

            var i = 0;
            for (var r = 0; r < rows; ++r)
            {
                for (var c = 0; c < cols; ++c)
                {
                    result[r, c] = arr[i++];
                }
            }

            return result;
        }
    }
}