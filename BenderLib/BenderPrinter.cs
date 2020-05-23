namespace BenderLib
{
    using System;
    using System.IO;
    
    /// <summary>
    /// Pretty printer for Benders
    /// </summary>
    public class BenderPrinter
    {
        // '-' means left align
        private static readonly string DefaultRowFormat = "{0,-20} {1,-16}" + Environment.NewLine;
        private static readonly string DefaultHeader = string.Format(DefaultRowFormat, "Element", "Value");
        private static readonly string DefaultLineDelimiter = new string('=', 80);

        /// <summary>
        ///     Create a new Bender pretty printer
        /// </summary>
        public BenderPrinter()
        {
            Header = DefaultHeader;
            RowFormat = DefaultRowFormat;
            LineDelimiter = DefaultLineDelimiter;
        }

        /// <summary>
        /// Gets or Sets the formatted Element table header
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or Sets the raw format. If you change this, consider
        /// also updating Header.
        /// </summary>
        public string RowFormat { get; set; }

        /// <summary>
        /// Long delimiter for splitting textual regions. Default
        /// is a 80 character string of '='
        /// </summary>
        public string LineDelimiter { get; set; }

        /// <summary>
        /// Writes bender to stream in a tabular format. When the write
        /// has completed, the stream position is set back to 0.
        /// </summary>
        /// <param name="bender">Data to write</param>
        /// <param name="stream">Where data is being written to</param>
        /// <exception cref="ArgumentException">Raised is bender or stream are null or if stream cannot be written</exception>
        public void WriteStream(Bender bender, Stream stream)
        {
            if (bender == null)
            {
                throw new ArgumentException("{0} cannot be null", nameof(bender));
            }

            if (stream == null || !stream.CanWrite)
            {
                throw new ArgumentException("{0} cannot be written", nameof(stream));
            }

            // Helper function wraps stream write operation
            void WriteBytes(string s)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(s);
                stream.Write(bytes, 0, bytes.Length);
            }

            WriteBytes(Header);
            WriteBytes(LineDelimiter);
            WriteBytes(Environment.NewLine);

            foreach (var f in bender.FormattedFields)
            {
                // Only print the name on the first row of this formatted element
                var isFirst = true;
                foreach (var v in f.Value)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        WriteBytes(string.Format(RowFormat, f.Name, v));
                    }
                    else
                    {
                        WriteBytes(string.Format(RowFormat, string.Empty, v));
                    }

                }
            }

            stream.Position = 0;
        }
    }
}
