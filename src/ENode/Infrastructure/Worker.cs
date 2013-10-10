using System;
using System.Threading;
using ENode.Infrastructure.Logging;

namespace ENode.Infrastructure
{
    /// <summary>Represent a background worker that will repeatedly execute a specific method.
    /// </summary>
    public class Worker
    {
        private bool _stopped;
        private readonly Action _action;
        private readonly Thread _thread;
        private readonly ILogger _logger;

        /// <summary>Return the IsAlive status of the current worker.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return _thread.IsAlive;
            }
        }
        /// <summary>Gets or sets the interval which the action executed.
        /// </summary>
        public int IntervalMilliseconds { get; set; }

        /// <summary>Initialize a new Worker for the specified method to run.
        /// </summary>
        /// <param name="action">The delegate method to execute in a loop.</param>
        public Worker(Action action) : this(action, 0)
        {
        }
        /// <summary>Initialize a new Worker for the specified method to run.
        /// </summary>
        /// <param name="action">The delegate method to execute in a loop.</param>
        /// <param name="intervalMilliseconds">The interval which the action executed.</param>
        public Worker(Action action, int intervalMilliseconds)
        {
            _action = action;
            IntervalMilliseconds = intervalMilliseconds;
            _thread = new Thread(Loop) { IsBackground = true };
            _thread.Name = string.Format("Worker thread {0}", _thread.ManagedThreadId);
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(_thread.Name);
        }

        /// <summary>Start the worker.
        /// </summary>
        public Worker Start()
        {
            if (!_thread.IsAlive)
            {
                _thread.Start();
            }
            return this;
        }
        /// <summary>Stop the worker.
        /// </summary>
        public Worker Stop()
        {
            _stopped = true;
            return this;
        }

        /// <summary>Executes the delegate method until the <see cref="Stop"/> method is called.
        /// </summary>
        private void Loop()
        {
            while (!_stopped)
            {
                try
                {
                    _action();
                    if (IntervalMilliseconds > 0)
                    {
                        Thread.Sleep(IntervalMilliseconds);
                    }
                }
                catch (ThreadAbortException abortException)
                {
                    _logger.Error("caught ThreadAbortException - resetting.", abortException);
                    Thread.ResetAbort();
                    _logger.Info("ThreadAbortException resetted.");
                }
                catch (Exception ex)
                {
                    _logger.Error("Exception raised when executing worker delegate.", ex);
                }
            }
        }
    }
}
