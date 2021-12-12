namespace Bender.Core.Rendering
{
    using Nodes;

    /// <summary>
    ///     Node renderer
    /// </summary>
    public interface IRenderProvider
    {
        /// <summary>
        /// Renders a node
        /// </summary>
        /// <param name="node">Node to render</param>
        void Render(BNode node);
    }
}