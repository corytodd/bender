namespace Bender.Core.Nodes
{
    using System;
    using System.IO;
    using Layouts;
    using Rendering;

    public class BMatrix<T> : BaseNode where T : IRenderable
    {
        private readonly T[,] _data;

        public BMatrix(Element el, T[,] data) : base(el)
        {
            _data = data;
        }

        public int RowCount => _data?.GetLength(0) ?? 0;

        public int ColCount => _data?.GetLength(1) ?? 0;

        public int ElementCount => _data?.Length ?? 0;

        public T this[int row, int col] => _data[row, col];

        public override string ToString()
        {
            return $"{Name} : ({RowCount}x{ColCount})";
        }

        /// <inheritdoc />
        public override void Render(StreamWriter stream)
        {
            if (_data is null)
            {
                stream.Write($"{this} : NULL");
                return;
            }

            stream.Write($"{Name} : ");

            for (var row = 0; row < RowCount; ++row)
            {
                for (var col = 0; col < ColCount; ++col)
                {
                    this[row, col].Render(stream);
                }
            }
        }
    }
}