namespace BenderLib
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// All bender errors are base on ParseException
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException(string fmt, params object[] args) : base(string.Format(fmt, args)) { }
    }

    /// <inheritdoc />
    /// <summary>
    /// Raised when an attempt is made to create too large a number from too small a buffer
    /// </summary>
    public class OutOfDataException : ParseException
    {
        public OutOfDataException(string fmt, params object[] args) : base(fmt, args)
        {
        }
    }
}
