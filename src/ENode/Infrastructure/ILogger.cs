using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a logger.
    /// </summary>
    public interface ILogger
    {
        bool IsDebugEnabled { get; }
        void Debug(object message);
        void DebugFormat(string format, params object[] args);
        void Debug(object message, Exception exception);

        void Info(object message);
        void InfoFormat(string format, params object[] args);
        void Info(object message, Exception exception);

        void Error(object message);
        void ErrorFormat(string format, params object[] args);
        void Error(object message, Exception exception);

        void Warn(object message);
        void WarnFormat(string format, params object[] args);
        void Warn(object message, Exception exception);

        void Fatal(object message);
        void FatalFormat(string format, params object[] args);
        void Fatal(object message, Exception exception);
    }
}