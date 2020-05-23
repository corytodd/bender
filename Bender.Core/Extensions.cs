
namespace Bender.Core
{
    using System;
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
    }
}
