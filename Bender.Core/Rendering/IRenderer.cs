namespace Bender.Core.Rendering;

using System.IO;
using Layouts;
using Nodes;

/// <summary>
///     Rendering implementation for a particular type
/// </summary>
/// <typeparam name="T">Type being rendered</typeparam>
public interface IRenderer<in T> where T : BNode
{
    /// <summary>
    ///     Write element's node value to stream
    /// </summary>
    /// <param name="el">Element descriptor</param>
    /// <param name="t">Node value</param>
    /// <param name="stream">Receives rendering</param>
    void Render(Element el, T t, StreamWriter stream);
}