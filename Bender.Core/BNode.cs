namespace Bender.Core
{
    using System.IO;

    public interface BNode
    {
        string Name { get; }
        
        void Print(StreamWriter writer);
    }
}