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
        private int _tabDepth;
        
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
                case BMatrix<IRenderable> matrix:
                    Render(matrix);
                    break;
                
                case BStructure structure:
                    Render(structure);
                    break;
                
                case BPrimitive<IRenderable> primitive:
                    Render(primitive);
                    break;

                default:
                    _streamWriter.WriteLine($"{new string('\t', _tabDepth)}{node}");
                    break;
            }
        }

        private void Render(BPrimitive<IRenderable> primitive)
        {
            _streamWriter.WriteLine($"{new string('\t', _tabDepth)}{primitive}");
        }

        /// <summary>
        /// Specialized matrix renderer
        /// </summary>
        /// <param name="matrix">Matrix to render</param>
        private void Render<T>(BMatrix<T> matrix) where T : IRenderable
        {
            _streamWriter.WriteLine($"{new string('\t', _tabDepth)}{matrix.Name} : ");
            ++_tabDepth;
            for(var row=0; row<matrix.RowCount; ++row)
            {
                var values = new List<string>(matrix.ColCount);
                for (var col = 0; col < matrix.ColCount; ++col)
                {
                    values.Add(matrix[row, col].Format());
                }
                _streamWriter.WriteLine($"{new string('\t', _tabDepth)}[{string.Join(',', values)}]");
            }

            --_tabDepth;
        }
        /// <summary>
        /// Specialized structure renderer
        /// </summary>
        /// <param name="structure">Structure to render</param>

        private void Render(BStructure structure)
        {
            _streamWriter.WriteLine($"{new string('\t', _tabDepth)}{structure.Name} : ");
            foreach (var field in structure.Fields)
            {
                ++_tabDepth;
                Render(field);
                --_tabDepth;
            }
        }
    }
}