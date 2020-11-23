#pragma warning disable 1591
namespace Bender.Core.Logging
{
    using System;

    public delegate bool Logger(LogLevel logLevel, Func<string> messageFunc, Exception exception = null,
        params object[] formatParameters);
}