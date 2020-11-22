namespace Bender.Core.Rendering
{
    using System.IO;

    public interface IRenderer<in T> where T : BNode
    {
        void Render(Element el, T t, StreamWriter stream);
    }
}