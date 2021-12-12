namespace Bender.Core;

using System;
using System.Collections.Generic;
using Nodes;

/// <summary>
///     Tree data structure
/// </summary>
/// <typeparam name="T">Tree type</typeparam>
public class ParseTree<T>
{
    private readonly LinkedList<ParseTree<T>> _children = new();

    /// <summary>
    ///     Create an empty ParseTree
    /// </summary>
    public ParseTree()
    {
    }

    /// <summary>
    ///     Create a new ParseTree with a root value
    /// </summary>
    /// <param name="node">Root value</param>
    public ParseTree(T node) : this(node, null)
    {
    }

    /// <summary>
    ///     Create a new ParseTree with the given value and parent
    /// </summary>
    /// <param name="node">Value</param>
    /// <param name="parent">Parent tree</param>
    private ParseTree(T node, ParseTree<T> parent)
    {
        Value = node;
        Parent = parent;
    }

    /// <summary>
    ///     Value of this node
    /// </summary>
    public T Value { get; }

    /// <summary>
    ///     Parent of this node
    /// </summary>
    public ParseTree<T> Parent { get; }

    /// <summary>
    ///     Children of this node
    /// </summary>
    public IEnumerable<ParseTree<T>> Children => _children;

    /// <summary>
    ///     Create new node for value, append to children, and return node
    /// </summary>
    /// <param name="value">Value to add</param>
    /// <returns>Node containing value</returns>
    public ParseTree<T> AddChild(T value)
    {
        var node = new ParseTree<T>(value, this);

        _children.AddLast(node);

        return node;
    }

    /// <summary>
    ///     Breadth first traversal of tree from current node
    /// </summary>
    /// <param name="visitNode">Called for each non-null child</param>
    public void Traverse(Action<T> visitNode)
    {
        if (Value is not null)
        {
            visitNode(Value);

            // Structures iterate over their own fields
            // so we can early return here.
            if (Value is BStructure)
            {
                return;
            }
        }

        foreach (var child in _children)
        {
            child.Traverse(visitNode);
        }
    }
}