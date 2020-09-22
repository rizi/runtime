// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Globalization;
using DiagnosticsTraceSource = System.Diagnostics.TraceSource;

namespace Microsoft.Extensions.Logging.TraceSource
{
    internal class TraceSourceLogger : ILogger
    {
        private readonly DiagnosticsTraceSource _traceSource;

        public TraceSourceLogger(DiagnosticsTraceSource traceSource)
        {
            _traceSource = traceSource;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            string message = string.Empty;
            if (formatter != null)
            {
                message = formatter(state, exception);
            }
            else
            {
                if (state != null)
                {
                    message += state;
                }
                if (exception != null)
                {
                    message += string.Format(CultureInfo.InvariantCulture, "{0}{1}", Environment.NewLine, exception);
                }
            }

            if (string.IsNullOrEmpty(message) && exception == null)
                return;

            if (exception != null)
            {
                string exceptionDelimiter = !string.IsNullOrEmpty(message) ? Environment.NewLine : string.Empty;
                message += string.Format(CultureInfo.InvariantCulture, "{0}Error: {1}", exceptionDelimiter, exception);
            }

            _traceSource.TraceEvent(GetEventType(logLevel), eventId.Id, message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None)
            {
                return false;
            }

            TraceEventType traceEventType = GetEventType(logLevel);
            return _traceSource.Switch.ShouldTrace(traceEventType);
        }

        private static TraceEventType GetEventType(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical: return TraceEventType.Critical;
                case LogLevel.Error: return TraceEventType.Error;
                case LogLevel.Warning: return TraceEventType.Warning;
                case LogLevel.Information: return TraceEventType.Information;
                case LogLevel.Trace:
                default: return TraceEventType.Verbose;
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new TraceSourceScope(state);
        }
    }
}
