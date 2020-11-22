namespace Bender.Core.Nodes
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using global::Bender.Core.Rendering;
    using Layouts;

    [DebuggerDisplay("{Name}")]
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
        public override void Render(StreamWriter stream)
        {
            if (_data is null)
            {
                stream.Write($"{this} : NULL");
                return;
            }

            stream.Write($"{Name} : ");
            foreach (var t in _data)
            {
                t.Render(stream);
            }
        }
    }
}