// ReSharper disable All

#pragma warning disable 1591
namespace Bender.Core.Logging.LogProviders
{
    using System;

#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif

    public abstract class LogProviderBase : ILogProvider
    {
        protected const string ErrorInitializingProvider =
            "Unable to log due to problem initializing the log provider. See inner exception for details.";

        private static readonly IDisposable NoopDisposableInstance = new DisposableAction();
        private readonly Lazy<OpenMdc> _lazyOpenMdcMethod;

        private readonly Lazy<OpenNdc> _lazyOpenNdcMethod;

        protected LogProviderBase()
        {
            _lazyOpenNdcMethod
                = new Lazy<OpenNdc>(GetOpenNdcMethod);
            _lazyOpenMdcMethod
                = new Lazy<OpenMdc>(GetOpenMdcMethod);
        }

        public abstract Logger GetLogger(string name);

        public IDisposable OpenNestedContext(string message)
        {
            return _lazyOpenNdcMethod.Value(message);
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            return _lazyOpenMdcMethod.Value(key, value, destructure);
        }

        protected virtual OpenNdc GetOpenNdcMethod()
        {
            return _ => NoopDisposableInstance;
        }

        protected virtual OpenMdc GetOpenMdcMethod()
        {
            return (_, __, ___) => NoopDisposableInstance;
        }

        protected delegate IDisposable OpenNdc(string message);

        protected delegate IDisposable OpenMdc(string key, object value, bool destructure);
    }
}