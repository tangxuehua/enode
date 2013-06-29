using System;
using System.Threading;

namespace ENode.Commanding
{
    public class CommandAsyncResult
    {
        private Action<CommandAsyncResult> _callback;

        public Exception Exception { get; private set; }

        public CommandAsyncResult(Action<CommandAsyncResult> callback)
        {
            _callback = callback;
        }

        public void Complete(Exception exception)
        {
            Exception = exception;
            if (_callback != null)
            {
                _callback(this);
            }
        }
    }
}
