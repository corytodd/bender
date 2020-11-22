namespace Bender.Core.Rendering
{
    using System.IO;
    using Layouts;
    using Nodes;

    public class ArrayRenderer<T> : IRenderer<BArrray<T>> where T : IRenderable
    {
        public void Render(Element el, BArrray<T> t, StreamWriter stream)
        {
            var f = el.GetCSharpFormatter();
            foreach (var value in t)
            {
                value.Print(stream);
            }
        }
    }
}