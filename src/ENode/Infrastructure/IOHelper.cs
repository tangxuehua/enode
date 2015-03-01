using System;
using System.Threading;
using System.Threading.Tasks;
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
        public void TryAsyncActionRecursively<TAsyncResult>(
            string asyncActionName,
            Func<Task<TAsyncResult>> asyncAction,
            Action<int> mainAction,
            Action<TAsyncResult> successAction,
            Func<string> getContextInfoFunc,
            Action failedAction,
            int retryTimes) where TAsyncResult : AsyncTaskResult
        {
            var retryAction = new Action<int>(currentRetryTimes =>
            {
                if (currentRetryTimes >= _immediatelyRetryTimes)
                {
                    Task.Factory.StartDelayedTask(_retryIntervalForIOException, () => mainAction(currentRetryTimes + 1));
                }
                else
                {
                    mainAction(currentRetryTimes + 1);
                }
            });
            var executeFailedAction = new Action(() =>
            {
                try
                {
                    if (failedAction != null)
                    {
                        failedAction();
                    }
                }
                catch (Exception unknownEx)
                {
                    _logger.Error(string.Format("Failed to execute the failedCallbackAction of asyncAction:{0}, contextInfo:{1}", asyncActionName, getContextInfoFunc()), unknownEx);
                }
            });
            var processTaskException = new Action<Exception, int>((ex, currentRetryTimes) =>
            {
                if (ex is IOException)
                {
                    _logger.Error(string.Format("Async task '{0}' has io exception, contextInfo:{1}, current retryTimes:{2}, try to run the async task again.", asyncActionName, getContextInfoFunc(), currentRetryTimes), ex);
                    retryAction(retryTimes);
                }
                else
                {
                    _logger.Error(string.Format("Async task '{0}' has unknown exception, contextInfo:{1}, current retryTimes:{2}", asyncActionName, getContextInfoFunc(), currentRetryTimes), ex);
                    executeFailedAction();
                }
            });
            var completeAction = new Action<Task<TAsyncResult>>(t =>
            {
                if (t.Exception != null)
                {
                    processTaskException(t.Exception.InnerException, retryTimes);
                    return;
                }
                if (t.IsCanceled)
                {
                    _logger.ErrorFormat("Async task '{0}' was cancelled, contextInfo:{1}, current retryTimes:{2}, try to run the async task again.", asyncActionName, getContextInfoFunc(), retryTimes);
                    retryAction(retryTimes);
                    return;
                }
                var result = t.Result;
                if (result == null)
                {
                    _logger.ErrorFormat("Async task '{0}' result is null, contextInfo:{1}, current retryTimes:{2}", asyncActionName, getContextInfoFunc(), retryTimes);
                    executeFailedAction();
                    return;
                }
                if (result.Status == AsyncTaskStatus.Success)
                {
                    if (successAction != null)
                    {
                        successAction(result);
                    }
                }
                else if (result.Status == AsyncTaskStatus.IOException)
                {
                    _logger.ErrorFormat("Async task '{0}' result status is io exception, contextInfo:{1}, current retryTimes:{2}, errorMsg:{3}, try to run the async task again.", asyncActionName, getContextInfoFunc(), retryTimes, result.ErrorMessage);
                    retryAction(retryTimes);
                }
                else if (result.Status == AsyncTaskStatus.Failed)
                {
                    _logger.ErrorFormat("Async task '{0}' was failed and will not be retry, contextInfo:{1}, current retryTimes:{2}, errorMsg:{3}", asyncActionName, getContextInfoFunc(), retryTimes, result.ErrorMessage);
                    executeFailedAction();
                }
            });

            try
            {
                asyncAction().ContinueWith(completeAction);
            }
            catch (IOException ex)
            {
                _logger.Error(string.Format("IOException raised when executing async task '{0}', contextInfo:{1}, current retryTimes:{2}, try to run the async task again.", asyncActionName, getContextInfoFunc(), retryTimes), ex);
                retryAction(retryTimes);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Unknown exception raised when executing async task '{0}', contextInfo:{1}, current retryTimes:{2}", asyncActionName, getContextInfoFunc(), retryTimes), ex);
                executeFailedAction();
            }
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
        public Task TryIOActionAsync(Func<Task> action, string actionName)
        {
            Ensure.NotNull(action, "action");
            Ensure.NotNull(actionName, "actionName");
            try
            {
                return action();
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
        public Task<T> TryIOFuncAsync<T>(Func<Task<T>> func, string funcName)
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
