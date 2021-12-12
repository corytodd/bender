namespace Bender.Core.Rendering
{
    using System.Collections.Generic;
    using System.IO;
    using Nodes;

    /// <summary>
    ///     Sane default renderer
    /// </summary>
    public class DefaultRenderProvider : IRenderProvider
    {
        private readonly StreamWriter _streamWriter;
        
        /// <summary>
        /// Create a new render provider
        /// </summary>
        /// <param name="streamWriter">Receives rendered content</param>
        public DefaultRenderProvider(StreamWriter streamWriter)
        {
            _streamWriter = streamWriter;
        }
        
        /// <summary>
        /// Render node
        /// </summary>
        /// <param name="node">Node to render</param>
        public void Render(BNode node)
        {
            switch (node)
            {
                case BMatrix<Number> matrix:
                    Render(matrix);
                    break;
                
                default:
                    _streamWriter.WriteLine(node);
                    break;
            }
        }

        /// <summary>
        /// Specialized matrix renderer
        /// </summary>
        /// <param name="matrix">Matrix to render</param>
        private void Render(BMatrix<Number> matrix)
        {
            _streamWriter.WriteLine($"{matrix.Name} : ");
            for(var row=0; row<matrix.RowCount; ++row)
            {
                var values = new List<string>(matrix.ColCount);
                for (var col = 0; col < matrix.ColCount; ++col)
                {
                    values.Add(matrix[row, col].Format());
                }
                _streamWriter.WriteLine($"[{string.Join(',', values)}]");
            }
        }
    }
}