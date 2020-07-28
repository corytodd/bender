namespace Bender.Core
{
    using System;
    using System.Collections.Generic;

    public class ParseTree<T>
    {
        private LinkedList<ParseTree<T>> _children = new LinkedList<ParseTree<T>>();

        public ParseTree()
        {
        }

        public ParseTree(T node) : this(node, null)
        {
        }

        private ParseTree(T node, ParseTree<T> parent)
        {
            Value = node;
            Parent = parent;
        }

        public T Value { get; }

        public ParseTree<T> Parent { get; }

        public IEnumerable<ParseTree<T>> Children
        {
            get => _children;
        }

        public ParseTree<T> AddChild(T value)
        {
            var node = new ParseTree<T>(value, this);

            _children.AddLast(node);

            return node;
        }
        
        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children)
            {
                child.Traverse(action);
            }
        }
    }
}