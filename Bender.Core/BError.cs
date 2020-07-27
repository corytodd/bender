namespace Bender.Core
{
    using System;

    public class BError : BNode
    {
        public BError(string error, string details, Exception ex = null)
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
            return $"{Error} : {Details}";
        }
    }
}