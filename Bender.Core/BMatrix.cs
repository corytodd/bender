namespace Bender.Core
{
    using System.IO;

    public class BMatrix<T> : BNode
    {
        private readonly T[,] _data;

        public BMatrix(string name, T[,] data)
        {
            Name = name;
            _data = data;
        }

        public string Name { get; }
        
        public int RowCount => _data?.GetLength(0) ?? 0;

        public int ColCount => _data?.GetLength(1) ?? 0;

        public int ElementCount => _data?.Length ?? 0;

        public override string ToString()
        {
            return $"{Name}:({RowCount}x{ColCount})";
        }

        public void Print(StreamWriter writer)
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