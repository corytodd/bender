namespace Bender.Core.Nodes
{
    using System.IO;
    using Layouts;

    public class BMatrix<T> : BaseNode
    {
        private readonly T[,] _data;

        public BMatrix(Element el, T[,] data) : base(el)
        {
            _data = data;
        }
        
        public int RowCount => _data?.GetLength(0) ?? 0;

        public int ColCount => _data?.GetLength(1) ?? 0;

        public int ElementCount => _data?.Length ?? 0;

        public override string ToString()
        {
            return $"{Name}:({RowCount}x{ColCount})";
        }

        /// <inheritdoc />
        public override void Print(StreamWriter writer)
        {
            if (_data is null)
            {
                writer.WriteLine($"{this}:NULL");
                return;
            }

            writer.Write($"{Name}:");

            for (var row = 0; row < RowCount; ++row)
            {
                for (var col = 0; col < ColCount; ++col)
                {
                    writer.Write($"{_data[row, col]}");
                }

                writer.WriteLine();
            }
            
            writer.WriteLine();
        }
    }
}