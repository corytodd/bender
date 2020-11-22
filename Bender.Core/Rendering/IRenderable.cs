namespace Bender.Core.Rendering
{
    using System.IO;

    /// <summary>
    ///     A type that can render itself
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        ///     Format self using specified format string
        /// </summary>
        /// <param name="format">Standard format string, e.g. {0} or {0:X4}</param>
        /// <returns></returns>
        string Format(string format);

        /// <summary>
        ///     Write self to stream
        /// </summary>
        /// <param name="stream">Receives rendered data</param>
        void Print(StreamWriter stream);
    }
}