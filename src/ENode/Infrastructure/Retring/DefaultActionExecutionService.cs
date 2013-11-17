using System;
using System.Collections.Concurrent;
using ENode.Infrastructure.Logging;

namespace ENode.Infrastructure.Retring
{
    /// <summary>The default implementation of IActionExecutionService.
    /// </summary>
    public class DefaultActionExecutionService : IActionExecutionService
    {
        private const int DefaultPeriod = 5000;
        private readonly BlockingCollection<ActionInfo> _actionQueue = new BlockingCollection<ActionInfo>(new ConcurrentQueue<ActionInfo>());
        private readonly Worker _worker;
        private readonly ILogger _logger;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public DefaultActionExecutionService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType().Name);
            _worker = new Worker(TryTakeAndExecuteAction, DefaultPeriod);
            _worker.Start();
        }

        /// <summary>Initialize the retry service.
        /// </summary>
        /// <param name="period"></param>
        public void Initialize(int period)
        {
            _worker.IntervalMilliseconds = period;
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
                _actionQueue.Add(new ActionInfo(actionName, obj => action(), null, nextAction));
            }
        }
        /// <summary>Try to execute the given action with the given max retry count. If success then returns true; otherwise, returns false.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public bool TryRecursively(string actionName, Func<bool> action, int maxRetryCount)
        {
            return TryRecursively(actionName, (x, y, z) => action(), 0, maxRetryCount);
        }

        private void TryTakeAndExecuteAction()
        {
            try
            {
                TryAction(_actionQueue.Take());
            }
            catch (Exception ex)
            {
                _logger.Error("Exception raised when retring action.", ex);
            }
        }
        private bool TryRecursively(string actionName, Func<string, int, int, bool> action, int retriedCount, int maxRetryCount)
        {
            var success = false;
            try
            {
                _logger.DebugFormat("Executing action {0}.", actionName);
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
                _logger.DebugFormat("Executed action {0}.", actionName);
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
                _logger.DebugFormat("Executing action {0}.", actionInfo.Name);
                success = actionInfo.Action(actionInfo.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when executing action {0}.", actionInfo.Name), ex);
            }
            finally
            {
                if (success)
                {
                    _logger.DebugFormat("Executed action {0}.", actionInfo.Name);
                    if (actionInfo.Next != null)
                    {
                        TryAction(actionInfo.Next);
                    }
                }
                else
                {
                    _actionQueue.Add(actionInfo);
                }
            }
        }
    }
}
