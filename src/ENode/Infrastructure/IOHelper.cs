using System;
using System.Threading;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Configurations;

namespace ENode.Infrastructure
{
    public class IOHelper
    {
        private readonly ILogger _logger;
        private readonly int _immediatelyRetryTimes;
        private readonly int _retryIntervalForIOException;

        public IOHelper(ILoggerFactory loggerFactory)
        {
            _immediatelyRetryTimes = ENodeConfiguration.Instance.Setting.ImmediatelyRetryTimes;
            _retryIntervalForIOException = ENodeConfiguration.Instance.Setting.RetryIntervalForIOException;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public IOActionResult TryIOActionRecursively(string actionName, Func<string> getContextInfo, Action action)
        {
            Ensure.NotNull(actionName, "actionName");
            Ensure.NotNull(getContextInfo, "getContextInfo");
            Ensure.NotNull(action, "action");
            return TryIOActionRecursivelyInternal(actionName, getContextInfo, (x, y, z) => action(), 0);
        }
        public IOFuncResult<T> TryIOFuncRecursively<T>(string funcName, Func<string> getContextInfo, Func<T> func)
        {
            Ensure.NotNull(funcName, "funcName");
            Ensure.NotNull(getContextInfo, "getContextInfo");
            Ensure.NotNull(func, "func");
            return TryIOFuncRecursivelyInternal(funcName, getContextInfo, (x, y, z) => func(), 0);
        }

        public void TryIOAction(Action action, string actionName)
        {
            Ensure.NotNull(action, "action");
            Ensure.NotNull(actionName, "actionName");
            try
            {
                action();
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new IOException(string.Format("{0} failed.", actionName), ex);
            }
        }
        public T TryIOFunc<T>(Func<T> func, string funcName)
        {
            Ensure.NotNull(func, "func");
            Ensure.NotNull(funcName, "funcName");
            try
            {
                return func();
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new IOException(string.Format("{0} failed.", funcName), ex);
            }
        }

        private IOActionResult TryIOActionRecursivelyInternal(string actionName, Func<string> getContextInfo, Action<string, Func<string>, long> action, long retryTimes)
        {
            try
            {
                action(actionName, getContextInfo, retryTimes);
                return IOActionResult.SuccessResult;
            }
            catch (IOException ex)
            {
                _logger.Error(string.Format("IOException raised when executing action '{0}', current retryTimes:{1}, contextInfo:{2}", actionName, retryTimes, getContextInfo()), ex);
                if (retryTimes > _immediatelyRetryTimes)
                {
                    Thread.Sleep(_retryIntervalForIOException);
                }
                retryTimes++;
                return TryIOActionRecursivelyInternal(actionName, getContextInfo, action, retryTimes);
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unknown exception raised when executing action '{0}', current retryTimes:{1}, contextInfo:{2}", actionName, retryTimes, getContextInfo());
                _logger.Error(errorMessage, ex);
                return new IOActionResult(false, ex);
            }
        }
        private IOFuncResult<T> TryIOFuncRecursivelyInternal<T>(string funcName, Func<string> getContextInfo, Func<string, Func<string>, long, T> func, long retryTimes)
        {
            try
            {
                var data = func(funcName, getContextInfo, retryTimes);
                return new IOFuncResult<T>(true, null, data);
            }
            catch (IOException ex)
            {
                _logger.Error(string.Format("IOException raised when executing func '{0}', current retryTimes:{1}, contextInfo:{2}", funcName, retryTimes, getContextInfo()), ex);
                if (retryTimes > _immediatelyRetryTimes)
                {
                    Thread.Sleep(_retryIntervalForIOException);
                }
                retryTimes++;
                return TryIOFuncRecursivelyInternal(funcName, getContextInfo, func, retryTimes);
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unknown exception raised when executing func '{0}', current retryTimes:{1}, contextInfo:{2}", funcName, retryTimes, getContextInfo());
                _logger.Error(errorMessage, ex);
                return new IOFuncResult<T>(false, ex, default(T));
            }
        }
    }
    public class IOActionResult
    {
        public bool Success { get; private set; }
        public Exception Exception { get; private set; }
        public static IOActionResult SuccessResult = new IOActionResult(true, null);

        public IOActionResult(bool success, Exception exception)
        {
            Success = success;
            if (!success)
            {
                Ensure.NotNull(exception, "exception");
            }
            Exception = exception;
        }
    }
    public class IOFuncResult<T> : IOActionResult
    {
        public T Data { get; private set; }

        public IOFuncResult(bool success, Exception exception, T data) : base(success, exception)
        {
            Data = data;
        }
    }
}
