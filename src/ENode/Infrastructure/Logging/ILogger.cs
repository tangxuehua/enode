using System;

namespace ENode.Infrastructure.Logging
{
    /// <summary>Represents a logger interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>Indicates whether the logger can write debug level log messages.
        /// </summary>
        bool IsDebugEnabled { get; }
        /// <summary>Write a debug level log message.
        /// </summary>
        /// <param name="message"></param>
        void Debug(object message);
        /// <summary>Write a debug level log message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void DebugFormat(string format, params object[] args);
        /// <summary>Write a debug level log message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Debug(object message, Exception exception);

        /// <summary>Write a info level log message.
        /// </summary>
        /// <param name="message"></param>
        void Info(object message);
        /// <summary>Write a info level log message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void InfoFormat(string format, params object[] args);
        /// <summary>Write a info level log message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Info(object message, Exception exception);

        /// <summary>Write an error level log message.
        /// </summary>
        /// <param name="message"></param>
        void Error(object message);
        /// <summary>Write an error level log message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void ErrorFormat(string format, params object[] args);
        /// <summary>Write an error level log message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Error(object message, Exception exception);

        /// <summary>Write a warnning level log message.
        /// </summary>
        /// <param name="message"></param>
        void Warn(object message);
        /// <summary>Write a warnning level log message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void WarnFormat(string format, params object[] args);
        /// <summary>Write a warnning level log message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Warn(object message, Exception exception);

        /// <summary>Write a fatal level log message.
        /// </summary>
        /// <param name="message"></param>
        void Fatal(object message);
        /// <summary>Write a fatal level log message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void FatalFormat(string format, params object[] args);
        /// <summary>Write a fatal level log message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Fatal(object message, Exception exception);
    }
}