namespace Bender.Core
{
    using System.IO;
    using Rendering;

    public interface BNode : IRenderable
    {
        public Element El { get; }
        
        string Name { get; }
    }
}