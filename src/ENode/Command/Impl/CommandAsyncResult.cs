using System;
using System.Threading;

namespace ENode.Commanding
{
    public class CommandAsyncResult
    {
        public bool IsCompleted { get; private set; }
        public ManualResetEvent WaitHandle { get; private set; }
        public Action<CommandAsyncResult> CompleteCallback { get; private set; }
        public Exception Exception { get; private set; }

        public CommandAsyncResult(ManualResetEvent waitHandle)
        {
            if (waitHandle == null)
            {
                throw new ArgumentNullException("waitHandle");
            }
            WaitHandle = waitHandle;
        }
        public CommandAsyncResult(Action<CommandAsyncResult> completeCallback)
        {
            CompleteCallback = completeCallback;
        }

        public void Complete(Exception exception)
        {
            IsCompleted = true;
            Exception = exception;

            if (WaitHandle != null)
            {
                WaitHandle.Set();
            }
            else if (CompleteCallback != null)
            {
                CompleteCallback(this);
            }
        }
    }
}
