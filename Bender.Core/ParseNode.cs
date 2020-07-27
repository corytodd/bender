namespace Bender.Core
{
    using System.Collections.Generic;

    public class ParseNode : BNode
    {
        public ParseNode(BNode value, ParseNode parent)
        {
            Value = value;
            Parent = parent;
            Children = new List<ParseNode>();
        }

        public BNode Value { get; }

        public ParseNode Parent { get; }

        public List<ParseNode> Children { get; }

        public ParseNode Add(BNode value)
        {
            var node = new ParseNode(value, this);
            Children.Add(node);
            return node;
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}