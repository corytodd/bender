namespace Bender.Core
{
    using System.IO;
    using System.Text;

    public class BArrray<T> : BaseNode
    {
        private T[] _data;

        public BArrray(Element el, T[] arr) : base(el)
        {
            _data = arr;
        }

        public int Length => _data?.Length ?? 0;

        public T this[int index]
        {
            get => _data[index];
        }

        public override string ToString()
        {
            return $"array({Length})";
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
            writer.WriteLine(string.Join(",", _data));
        }
    }
}