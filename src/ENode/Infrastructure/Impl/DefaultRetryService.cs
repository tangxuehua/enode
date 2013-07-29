using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ENode.Infrastructure
{
    public class DefaultRetryService : IRetryService
    {
        private const long DefaultPeriod = 5000;
        private BlockingCollection<ActionInfo> _retryQueue = new BlockingCollection<ActionInfo>(new ConcurrentQueue<ActionInfo>());
        private Timer _timer;
        private ILogger _logger;
        private bool _looping;

        public DefaultRetryService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType().Name);
            _timer = new Timer(Loop, null, 0, DefaultPeriod);
        }

        public void Initialize(long period)
        {
            _timer.Change(0, period);
        }
        public bool TryAction(string actionName, Func<bool> action, int maxRetryCount)
        {
            return TryRecursively(actionName, (x, y, z) => action(), 0, maxRetryCount);
        }
        public void RetryInQueue(ActionInfo actionInfo)
        {
            _retryQueue.Add(actionInfo);
        }

        private void Loop(object data)
        {
            try
            {
                if (!_looping)
                {
                    _looping = true;
                    RetryAction();
                    _looping = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Exception raised when retring action.", ex);
                _looping = false;
            }
        }
        private bool TryRecursively(string actionName, Func<string, int, int, bool> action, int retriedCount, int maxRetryCount)
        {
            var success = false;
            try
            {
                success = action(actionName, retriedCount, maxRetryCount);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when tring action {0}.", actionName), ex);
            }

            if (success)
            {
                return true;
            }
            else if (retriedCount < maxRetryCount)
            {
                var result = TryRecursively(actionName, action, retriedCount + 1, maxRetryCount);
                _logger.InfoFormat("Retried action {0} for {1} times.", actionName, retriedCount + 1);
                return result;
            }
            else
            {
                return false;
            }
        }
        private void RetryAction()
        {
            var actionInfo = _retryQueue.Take();
            if (actionInfo != null)
            {
                var success = false;
                try
                {
                    success = actionInfo.Action(actionInfo.Data);
                    _logger.InfoFormat("Retried action {0} from queue.", actionInfo.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when retring action {0}.", actionInfo.Name), ex);
                }
                finally
                {
                    if (success)
                    {
                        if (actionInfo.Next != null)
                        {
                            _retryQueue.Add(actionInfo.Next);
                        }
                    }
                    else
                    {
                        _retryQueue.Add(actionInfo);
                    }
                }
            }
        }
    }
}
