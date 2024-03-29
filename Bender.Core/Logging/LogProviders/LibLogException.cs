#pragma warning disable 1591
namespace Bender.Core.Logging.LogProviders;

using System;

public class LibLogException : Exception
{
    public LibLogException(string message)
        : base(message)
    {
    }

    public LibLogException(string message, Exception inner)
        : base(message, inner)
    {
    }
}