namespace Bender.Core
{
    using System;
    using System.IO;

    public class BError : BNode
    {
        public BError(string error, string details, Exception ex = null)
        {
            Error = error;
            Details = details;
            Exception = ex;
        }   
        
        public string Name { get; }

        public Exception Exception { get; }

        public string Error { get; }

        public string Details { get; }

        public override string ToString()
        {
            return $"{Name}:{Error}|{Details}";
        }

        public void Print(StreamWriter writer)
        {
            writer.WriteLine(this);
        }
    }
}