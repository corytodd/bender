namespace Bender.Core
{
    public class BArrray<T> : BNode
    {
        private T[] _data;

        public BArrray(T[] arr)
        {
            _data = arr;
        }

        public int Length { get; }

        public T this[int index]
        {
            get => _data[index];
        }

        public override string ToString()
        {
            return $"array({Length})";
        }
    }
}