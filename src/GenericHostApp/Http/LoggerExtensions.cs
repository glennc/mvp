using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GenericHostApp.Http
{
    internal static class LoggerEventIds
    {
        public const int RequestStarting = 1;
        public const int RequestFinished = 2;
        public const int Starting = 3;
        public const int Started = 4;
        public const int Shutdown = 5;
        public const int ApplicationStartupException = 6;
        public const int ApplicationStoppingException = 7;
        public const int ApplicationStoppedException = 8;
        public const int HostedServiceStartException = 9;
        public const int HostedServiceStopException = 10;
        public const int HostingStartupAssemblyException = 11;
        public const int ServerShutdownException = 12;
    }

    internal static class HostingLoggerExtensions
    {
        public static IDisposable RequestScope(this ILogger logger, HttpContext httpContext, string correlationId)
        {
            return logger.BeginScope(new HostingLogScope(httpContext, correlationId));
        }

        public static void ApplicationError(this ILogger logger, Exception exception)
        {
            logger.ApplicationError(
                eventId: LoggerEventIds.ApplicationStartupException,
                message: "Application startup exception",
                exception: exception);
        }

        public static void HostingStartupAssemblyError(this ILogger logger, Exception exception)
        {
            logger.ApplicationError(
                eventId: LoggerEventIds.HostingStartupAssemblyException,
                message: "Hosting startup assembly exception",
                exception: exception);
        }

        public static void ApplicationError(this ILogger logger, EventId eventId, string message, Exception exception)
        {
            var reflectionTypeLoadException = exception as ReflectionTypeLoadException;
            if (reflectionTypeLoadException != null)
            {
                foreach (var ex in reflectionTypeLoadException.LoaderExceptions)
                {
                    message = message + Environment.NewLine + ex.Message;
                }
            }

            logger.LogCritical(
                eventId: eventId,
                message: message,
                exception: exception);
        }

        public static void Starting(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                   eventId: LoggerEventIds.Starting,
                   message: "Hosting starting");
            }
        }

        public static void Started(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    eventId: LoggerEventIds.Started,
                    message: "Hosting started");
            }
        }

        public static void Shutdown(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    eventId: LoggerEventIds.Shutdown,
                    message: "Hosting shutdown");
            }
        }

        public static void ServerShutdownException(this ILogger logger, Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    eventId: LoggerEventIds.ServerShutdownException,
                    exception: ex,
                    message: "Server shutdown exception");
            }
        }

        private class HostingLogScope : IReadOnlyList<KeyValuePair<string, object>>
        {
            private readonly HttpContext _httpContext;
            private readonly string _correlationId;

            private string _cachedToString;

            public int Count
            {
                get
                {
                    return 3;
                }
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return new KeyValuePair<string, object>("RequestId", _httpContext.TraceIdentifier);
                    }
                    else if (index == 1)
                    {
                        return new KeyValuePair<string, object>("RequestPath", _httpContext.Request.Path.ToString());
                    }
                    else if (index == 2)
                    {
                        return new KeyValuePair<string, object>("CorrelationId", _correlationId);
                    }

                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }

            public HostingLogScope(HttpContext httpContext, string correlationId)
            {
                _httpContext = httpContext;
                _correlationId = correlationId;
            }

            public override string ToString()
            {
                if (_cachedToString == null)
                {
                    _cachedToString = string.Format(
                        CultureInfo.InvariantCulture,
                        "RequestId:{0} RequestPath:{1}",
                        _httpContext.TraceIdentifier,
                        _httpContext.Request.Path);
                }

                return _cachedToString;
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
