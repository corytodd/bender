namespace Bender.Core
{
    using System.IO;

    public interface BNode
    {
        public Element El { get; }
        
        string Name { get; }
        
        void Print(StreamWriter writer);
    }
}