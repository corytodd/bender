using System.Linq;

namespace BenderLib
{
    using System.Collections.Generic;
    using System.Text;

    public class Matrix
    {
        public delegate string Formatter(Element el, byte[] data);

        public string Name { get; set; }

        public int Columns { get; set; }

        public int Units { get; set; }

        public IEnumerable<string> Format(Element el, byte[] data, Formatter formatter)
        {
            if (Units == 0)
            {
                return Enumerable.Empty<string>();
            }

            var value = new List<string>();
            var sb = new StringBuilder();
            sb.Append("[ ");

            var totalVars = data.Length / Units;

            // Chop data into payload.unit bytes
            var cols = 0;
            var count = 0;
            foreach (var unit in data.AsChunks(Units))
            {
                ++count;

                sb.AppendFormat("{0} ", formatter.Invoke(el, unit));
                if (++cols % Columns != 0) continue;

                sb.Append("]");
                value.Add(sb.ToString());
                sb.Clear();

                if (count != totalVars)
                {
                    sb.Append("[ ");
                }
            }

            return value;
        }
    }
}
