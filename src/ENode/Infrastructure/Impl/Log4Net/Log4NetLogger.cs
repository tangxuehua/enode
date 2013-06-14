using System;
using ENode.Infrastructure;
using log4net;

namespace ENode.ThirdParty
{
    /// <summary>
    /// Log4Net提供的日志记录器
    /// </summary>
    public class Log4NetLogger : ILogger
    {
        private ILog _log;

        public Log4NetLogger(ILog log)
        {
            _log = log;
        }

        #region ILogger Members

        public void Info(object message)
        {
            _log.Info(message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            _log.InfoFormat(format, args);
        }

        public void Info(object message, Exception exception)
        {
            _log.Info(message, exception);
        }

        public void Error(object message)
        {
            _log.Error(message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _log.ErrorFormat(format, args);
        }

        public void Error(object message, Exception exception)
        {
            _log.Error(message, exception);
        }

        public void Warn(object message)
        {
            _log.Warn(message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            _log.WarnFormat(format, args);
        }

        public void Warn(object message, Exception exception)
        {
            _log.Warn(message, exception);
        }

        public bool IsDebugEnabled
        {
            get
            {
                return _log.IsDebugEnabled;
            }
        }

        public void Debug(object message)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug(message);
            }
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat(format, args);
            }
        }

        public void Debug(object message, Exception exception)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug(message, exception);
            }
        }

        public void Fatal(object message)
        {
            _log.Fatal(message);
        }

        public void FatalFormat(string format, params object[] args)
        {
            _log.FatalFormat(format, args);
        }

        public void Fatal(object message, Exception exception)
        {
            _log.Fatal(message, exception);
        }

        #endregion
    }
}