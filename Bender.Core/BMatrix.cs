namespace Bender.Core
{
    public class BMatrix<T> : BNode
    {
        private readonly T[,] _data;

        public BMatrix(T[,] data)
        {
            _data = data;
        }

        public int RowCount => _data.GetLength(0);

        public int ColCount => _data.GetLength(1);

        public int ElementCount => _data.Length;

        public override string ToString()
        {
            return $"matrix({RowCount}x{ColCount})";
        }
    }
}