using System;

namespace ENode.Infrastructure.Logging
{
    /// <summary>An empty logger which log nothing.
    /// </summary>
    public class EmptyLogger : ILogger
    {
        #region ILogger Members

        /// <summary>Returns false.
        /// </summary>
        public bool IsDebugEnabled
        {
            get
            {
                return false;
            }
        }
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message"></param>
        public void Debug(object message)
        {
        }
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void DebugFormat(string format, params object[] args)
        {
        }
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Debug(object message, Exception exception)
        {
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message"></param>
        public void Info(object message)
        {
        }
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void InfoFormat(string format, params object[] args)
        {
        }
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Info(object message, Exception exception)
        {
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message"></param>
        public void Error(object message)
        {
        }
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void ErrorFormat(string format, params object[] args)
        {
        }
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Error(object message, Exception exception)
        {
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message"></param>
        public void Warn(object message)
        {
        }
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WarnFormat(string format, params object[] args)
        {
        }
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Warn(object message, Exception exception)
        {
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message"></param>
        public void Fatal(object message)
        {
        }
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void FatalFormat(string format, params object[] args)
        {
        }
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Fatal(object message, Exception exception)
        {
        }

        #endregion
    }
}