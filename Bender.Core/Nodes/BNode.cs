namespace Bender.Core.Nodes
{
    using Layouts;
    using Rendering;

    /// <summary>
    ///     A BenderNode associates an <see cref="Element"/> with parsed data
    /// </summary>
    public interface BNode : IRenderable
    {
        /// <summary>
        ///     Parent Element describes node layout
        /// </summary>
        public Element El { get; }

        /// <summary>
        ///     Name of this node is derived from its <see cref="SpecFile"/>
        /// </summary>
        string Name { get; }
    }
}