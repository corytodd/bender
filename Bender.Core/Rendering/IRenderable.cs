namespace Bender.Core.Rendering;

using System.IO;

/// <summary>
///     A type that can render itself
/// </summary>
public interface IRenderable
{
    /// <summary>
    ///     Format self
    /// </summary>
    /// <returns>Formatted string</returns>
    string Format();

    /// <summary>
    ///     Write self to stream
    /// </summary>
    /// <param name="stream">Receives rendered data</param>
    void Render(StreamWriter stream);
}