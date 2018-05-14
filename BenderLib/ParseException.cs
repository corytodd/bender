namespace BenderLib
{
    using System;

    public class ParseException : Exception
    {
        public ParseException(string message) : base(message) { }
    }
}
