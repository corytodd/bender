namespace Bender.Core
{
    using System.Collections.Generic;

    public class ParseTree
    {
        private Stack<ParseNode> _stack = new Stack<ParseNode>();

        public List<ParseNode> Nodes { get; } = new List<ParseNode>();

        public ParseTree Begin(BNode val)
        {
            if (_stack.Count == 0)
            {
                var node = new ParseNode(val, null);
                Nodes.Add(node);
                _stack.Push(node);
            }
            else
            {
                var node = _stack.Peek().Add(val);
                _stack.Push(node);
            }

            return this;
        }

        public ParseTree Add(BNode val)
        {
            if (_stack.TryPeek(out var parent))
            {
                parent.Add(val);
            }
            else
            {
                Begin(val);
            }

            return this;
        }

        public ParseTree End()
        {
            _stack.Pop();
            return this;
        }
    }
}