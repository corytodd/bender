namespace Bender.Core.Nodes
{
    using System;
    using System.IO;

    public class BError : BaseNode
    {
        public BError(string error, string details, Exception ex = null) : base(null)
        {
            Error = error;
            Details = details;
            Exception = ex;
        }

        public Exception Exception { get; }

        public string Error { get; }

        public string Details { get; }

        public override string ToString()
        {
            return Format();
        }

        public override string Format()
        {
            return $"{Name} : {Error}|{Details}";
        }

        /// <inheritdoc />
        public override void Render(StreamWriter stream)
        {
            stream.Write(this);
        }
    }
}