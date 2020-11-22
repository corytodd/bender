namespace Bender.Core.Nodes
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using global::Bender.Core.Rendering;
    using Layouts;

    public class BArrray<T> : BaseNode, IEnumerable<T> where T : IRenderable
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

        public IEnumerator<T> GetEnumerator()
        {
            return new List<T>(_data).GetEnumerator();
        }

        public override string ToString()
        {
            return $"array({Length})";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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