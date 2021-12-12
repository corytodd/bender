namespace Bender.Core.Nodes;

using System.Diagnostics.CodeAnalysis;
using Layouts;

/// <summary>
///     A BenderNode associates an <see cref="Element" /> with parsed data
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface BNode
{
    /// <summary>
    ///     Parent Element describes node layout
    /// </summary>
    Element El { get; }

    /// <summary>
    ///     Name of this node is derived from its <see cref="SpecFile" />
    /// </summary>
    string Name { get; }
}