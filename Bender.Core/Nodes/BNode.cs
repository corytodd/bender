namespace Bender.Core.Nodes
{
    using Layouts;
    using Rendering;

    public interface BNode : IRenderable
    {
        public Element El { get; }
        
        string Name { get; }
    }
}