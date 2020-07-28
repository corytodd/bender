namespace Bender.Core
{
    using System.IO;
    using System.Text;

    public class BArrray<T> : BNode
    {
        private T[] _data;

        public BArrray(T[] arr)
        {
            _data = arr;
        }

        public string Name { get; }

        public int Length => _data?.Length ?? 0;

        public T this[int index]
        {
            get => _data[index];
        }

        public override string ToString()
        {
            return $"array({Length})";
        }

        public void Print(StreamWriter writer)
        {
            if (_data is null)
            {
                writer.WriteLine($"{this}:NULL");
                return;
            }

            writer.Write($"{Name}:");
            writer.WriteLine(",", _data);
        }
    }
}