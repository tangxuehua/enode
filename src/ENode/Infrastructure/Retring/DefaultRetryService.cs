using System;
using System.Collections.Concurrent;
using System.Threading;
using ENode.Infrastructure.Logging;

namespace ENode.Infrastructure.Retring
{
    /// <summary>The default implementation of IRetryService;
    /// </summary>
    public class DefaultRetryService : IRetryService
    {
        private const long DefaultPeriod = 5000;
        private readonly BlockingCollection<ActionInfo> _retryQueue = new BlockingCollection<ActionInfo>(new ConcurrentQueue<ActionInfo>());
        private readonly Timer _timer;
        private readonly ILogger _logger;
        private bool _looping;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public DefaultRetryService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType().Name);
            _timer = new Timer(Loop, null, 0, DefaultPeriod);
        }

        /// <summary>Initialize the retry service.
        /// </summary>
        /// <param name="period"></param>
        public void Initialize(long period)
        {
            _timer.Change(0, period);
        }
        /// <summary>Try to execute the given action with the given max retry count.
        /// <remarks>If the action execute still failed within the max retry count, then put the action into the retry queue;</remarks>
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <param name="nextAction"></param>
        public void TryAction(string actionName, Action action, int maxRetryCount, Action nextAction)
        {
            TryAction(actionName, () => { action(); return true; }, maxRetryCount, nextAction != null ? new ActionInfo("NextActionOf" + actionName, obj => { nextAction(); return true; }, null, null) : null);
        }
        /// <summary>Try to execute the given action with the given max retry count.
        /// <remarks>If the action execute still failed within the max retry count, then put the action into the retry queue;</remarks>
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <param name="nextAction"></param>
        public void TryAction(string actionName, Func<bool> action, int maxRetryCount, Action nextAction)
        {
            TryAction(actionName, action, maxRetryCount, nextAction != null ? new ActionInfo("NextActionOf" + actionName, obj => { nextAction(); return true; }, null, null) : null);
        }
        /// <summary>Try to execute the given action with the given max retry count.
        /// <remarks>If the action execute still failed within the max retry count, then put the action into the retry queue;</remarks>
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <param name="nextAction"></param>
        public void TryAction(string actionName, Func<bool> action, int maxRetryCount, Func<bool> nextAction)
        {
            TryAction(actionName, action, maxRetryCount, nextAction != null ? new ActionInfo("NextActionOf" + actionName, obj => nextAction(), null, null) : null);
        }
        /// <summary>Try to execute the given action with the given max retry count.
        /// <remarks>If the action execute still failed within the max retry count, then put the action into the retry queue;</remarks>
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <param name="nextAction"></param>
        public void TryAction(string actionName, Func<bool> action, int maxRetryCount, ActionInfo nextAction)
        {
            if (TryRecursively(actionName, (x, y, z) => action(), 0, maxRetryCount))
            {
                TryAction(nextAction);
            }
            else
            {
                _retryQueue.Add(new ActionInfo(actionName, obj => action(), null, nextAction));
            }
        }

        private void Loop(object data)
        {
            try
            {
                if (_looping) return;
                _looping = true;
                TryAction(_retryQueue.Take());
                _looping = false;
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
                if (retriedCount > 0)
                {
                    _logger.InfoFormat("Retried action {0} for {1} times.", actionName, retriedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when tring action {0}, retrid count {1}.", actionName, retriedCount), ex);
            }

            if (success)
            {
                return true;
            }
            if (retriedCount < maxRetryCount)
            {
                return TryRecursively(actionName, action, retriedCount + 1, maxRetryCount);
            }
            return false;
        }
        private void TryAction(ActionInfo actionInfo)
        {
            if (actionInfo == null) return;
            var success = false;
            try
            {
                success = actionInfo.Action(actionInfo.Data);
                _logger.InfoFormat("Executed action {0}.", actionInfo.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when executing action {0}.", actionInfo.Name), ex);
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
