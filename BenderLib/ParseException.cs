namespace BenderLib
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// All bender errors are base on ParseException
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message) { }
    }
}
