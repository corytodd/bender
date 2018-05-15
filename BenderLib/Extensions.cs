
namespace BenderLib
{
    using System;
    using System.Collections.Generic;

    public static class Extensions
    {

        /// <summary>
        /// Splits <paramref name="source"/> into chunks of size not greater than <paramref name="chunkMaxSize"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
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
    }
}
