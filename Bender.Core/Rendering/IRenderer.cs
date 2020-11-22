namespace Bender.Core.Rendering
{
    using System.IO;
    using Layouts;
    using Nodes;

    public interface IRenderer<in T> where T : BNode
    {
        void Render(Element el, T t, StreamWriter stream);
    }
}