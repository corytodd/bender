namespace Bender.Core.Nodes
{
    using global::Bender.Core.Rendering;
    using Layouts;

    public interface BNode : IRenderable
    {
        public Element El { get; }
        
        string Name { get; }
    }
}