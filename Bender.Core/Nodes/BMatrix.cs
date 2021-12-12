namespace Bender.Core.Nodes
{
    using Layouts;
    using Rendering;

    /// <summary>
    ///     A two dimension collection of elements
    /// </summary>
    /// <typeparam name="T">Matrix type</typeparam>
    public class BMatrix<T> : BaseNode where T : IRenderable
    {
        private readonly T[,] _data;

        /// <summary>
        ///     Create a new matrix
        /// </summary>
        /// <param name="el">Parent</param>
        /// <param name="data">Matrix elements</param>
        public BMatrix(Element el, T[,] data) : base(el)
        {
            Ensure.IsNotNull(nameof(data), data);
            _data = data;
        }

        /// <summary>
        ///     Count of rows in this matrix
        /// </summary>
        public int RowCount => _data.GetLength(0);

        /// <summary>
        ///     Count of columns in this matrix
        /// </summary>
        public int ColCount => _data.GetLength(1);

        /// <summary>
        ///     Total count of elements in this matrix
        /// </summary>
        public int ElementCount => _data.Length;

        /// <summary>
        ///     Returns elements at specified row and column
        /// </summary>
        /// <param name="row">Matrix row</param>
        /// <param name="col">Matrix column</param>
        public T this[int row, int col] => _data[row, col];

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} : ({RowCount}x{ColCount})";
        }
    }
}