namespace Bender.Core.Rendering
{
    using System.IO;
    using Nodes;

    public class DefaultRenderProvider : IRenderProvider
    {
        private readonly StreamWriter _streamWriter;
        
        public DefaultRenderProvider(StreamWriter streamWriter)
        {
            _streamWriter = streamWriter;
        }
        
        public void Render(BNode node)
        {
            _streamWriter.WriteLine(node);
        }
    }
}