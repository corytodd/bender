// ReSharper disable MemberCanBePrivate.Global - This is a library class, consumers may instantiate it

namespace Bender.Core;

using System;
using System.IO;
using Nodes;
using Rendering;

/// <summary>
///     Pretty printer for Benders
/// </summary>
public class BenderPrinter
{
    // '-' means left align
    private static readonly string DefaultHeader = string.Empty;
    private static readonly string DefaultLineDelimiter = new('=', 80);

    /// <summary>
    ///     Create a new Bender pretty printer
    /// </summary>
    public BenderPrinter()
    {
        Header = DefaultHeader;
        LineDelimiter = DefaultLineDelimiter;
    }

    /// <summary>
    ///     Gets or Sets the formatted Element table header
    /// </summary>
    public string Header { get; set; }

    /// <summary>
    ///     Long delimiter for splitting textual regions. Default
    ///     is a 80 character string of '='
    /// </summary>
    public string LineDelimiter { get; set; }

    /// <summary>
    ///     Writes bender to stream in a tabular format. When the write
    ///     has completed, the stream position is set back to 0.
    /// </summary>
    /// <param name="bender">Data to write</param>
    /// <param name="stream">Where data is being written to</param>
    /// <exception cref="ArgumentException">Raised is bender or stream are null or if stream cannot be written</exception>
    public void WriteStream(Bender bender, StreamWriter stream)
    {
        Ensure.IsNotNull(nameof(bender), bender);
        Ensure.IsNotNull(nameof(stream), stream);
        Ensure.IsValid(nameof(stream), stream.BaseStream.CanWrite, $"{nameof(stream)} cannot be written");

        var renderProvider = new DefaultRenderProvider(stream);


        void RenderNode(BNode node)
        {
            renderProvider.Render(node);
        }

        stream.Write(Header);
        stream.Write(LineDelimiter);
        stream.Write(Environment.NewLine);

        bender.Tree.Traverse(RenderNode);
    }
}