namespace Bender.Core.Rendering
{
    using Nodes;

    public interface IRenderProvider
    {
        void Render(BNode node);
    }
}