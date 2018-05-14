namespace BenderLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Reads a binary file and formats the data as specified in the SpecFile
    /// </summary>
    public class BinaryParser
    {
        private readonly SpecFile _mSpec;

        /// <summary>
        /// Creates a new parser
        /// </summary>
        /// <param name="spec">Binary layout spec</param>
        public BinaryParser(SpecFile spec)
        {
            _mSpec = spec;
        }

        /// <summary>
        /// Perform the parse operation against this binary
        /// </summary>
        /// <param name="binary">Data to parse</param>
        /// <returns>Parsed result</returns>
        public Bender Parse(DataFile binary)
        {
            var bender = new Bender(_mSpec);

            var hostIsLittleEndian = BitConverter.IsLittleEndian;

            using(var stream = new MemoryStream(binary.Data))
            using (var reader = new BinaryReader(stream))
            {
                foreach(var el in _mSpec.Elements)
                {
                    var buff = reader.ReadBytes(el.Width);                    
                        
                    // If byte order does not match, flip now
                    if(!(el.LittleEndian && hostIsLittleEndian))
                    {
                        Array.Reverse(buff);
                    }

                    var formatted = FormatBuffer(el, buff);

                    // '-' means left align
                    bender.FormattedFields.Add(formatted);
                }
            }

            return bender;
        }

        /// <summary>
        /// Formats data according to element rules
        /// </summary>
        /// <param name="el">Element rules</param>
        /// <param name="data">Data to format</param>
        /// <returns>Formatted string</returns>
        private Bender.FormattedField FormatBuffer(Element el, byte[] data)
        {
            if (el.Elide)
            {
                return new Bender.FormattedField
                {
                    Name = el.Name,
                    Value = new List<string>
                    { string.Format("Elided {0} btyes", data.Length) }
                };
            }
        
            var value = new List<string>();

            if (string.IsNullOrEmpty(el.Matrix))
            {
                var number = Number.From(el, data);

                switch (el.Format)
                {
                    case ElementFormat.Binary:
                        // Make sure every byte has 8 places, 0 filled if needed
                        var binary = Convert.ToString(number.sl, 2).PadLeft(el.Width * 8, '0');
                        value.Add(string.Format("b{0}", binary));
                        break;
                    case ElementFormat.Octal:
                        value.Add(string.Format("O{0}", Convert.ToString(number.sl, 8)));
                        break;
                    case ElementFormat.Decimal:
                        value.Add(number.sl.ToString());
                        break;
                    case ElementFormat.Hex:
                        value.Add(string.Format("0x{0:X}", number.sl));
                        break;
                    case ElementFormat.ASCII:
                        value.Add(Encoding.ASCII.GetString(data));
                        break;
                    case ElementFormat.UTF16:
                        value.Add(Encoding.Unicode.GetString(data));
                        break;
                    case ElementFormat.Float:
                        value.Add(number.d.ToString("#.#####"));
                        break;
                }
            }
            else
            {
                value.AddRange(FormatMatrix(el, data));
            }

            return new Bender.FormattedField
            {
                Name = el.Name,
                Value = value
            };
        }

        /// <summary>
        /// Formats the nested payload type as a matrix
        /// </summary>
        /// <param name="el">Element descriptor</param>
        /// <param name="data">Raw data</param>
        /// <returns>List of formatted matrix rows</returns>
        private IEnumerable<string> FormatMatrix(Element el, byte[] data)
        {            
            if (_mSpec.Matrices == null)
            {
                return new List<string> { string.Format("No matrices specified but element {0} has referenced one", el.Name)};
            }

            var payload = _mSpec.Matrices.FirstOrDefault(p => el.Matrix.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if(payload == null)
            {
                return new List<string> { string.Format("Unknown payload type {0} on element {1}", el.Matrix, el.Name) };
            }

            // Make a copy of Element and erase the payload name so we don't get stuck in a recursive loop
            var elClone = el.Clone();
            elClone.Matrix = string.Empty;
            elClone.Width = payload.Units;

            string Formatter(Element e, byte[] d)
            {
                return FormatBuffer(e, d).Value.First();
            }

            return payload.Format(elClone, data, Formatter);
        }
       
    }
}
