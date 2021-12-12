namespace Bender.Core.Nodes;

using System.Diagnostics;
using Layouts;

/// <summary>
///     BaseNode common functionality
/// </summary>
[DebuggerDisplay("{" + nameof(Name) + "}")]
public abstract class BaseNode : BNode
{
    /// <summary>
    ///     Create a new Node
    /// </summary>
    /// <param name="el">Parent</param>
    protected BaseNode(Element el)
    {
        El = el;
        Name = el?.Name;
    }

    /// <inheritdoc />
    public Element El { get; }

    /// <inheritdoc />
    public string Name { get; }
}