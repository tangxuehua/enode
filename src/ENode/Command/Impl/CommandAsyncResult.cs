using System;
using System.Threading;
using ENode.Infrastructure;

namespace ENode.Commanding {
    public class CommandAsyncResult {
        private ManualResetEvent _waitHandle;
        private Action<CommandAsyncResult> _callback;

        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; }
        public Exception Exception { get; private set; }

        public CommandAsyncResult(ManualResetEvent waitHandle) {
            if (waitHandle == null) {
                throw new ArgumentNullException("waitHandle");
            }
            _waitHandle = waitHandle;
        }
        public CommandAsyncResult(Action<CommandAsyncResult> callback) {
            _callback = callback;
        }

        public void Complete(string errorMessage, Exception exception) {
            IsCompleted = true;
            ErrorMessage = errorMessage;
            Exception = exception;

            if (_waitHandle != null) {
                _waitHandle.Set();
            }
            else if (_callback != null) {
                _callback(this);
            }
        }
        public bool HasError {
            get {
                return ErrorMessage != null || Exception != null;
            }
        }
    }
}
