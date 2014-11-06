namespace ENode.Infrastructure.Impl
{
    public abstract class ProcessingContext<TMessage, TResult> : IProcessingContext where TMessage : class
    {
        public string ProcessName { get; private set; }
        public TMessage Message { get; private set; }
        public IMessageProcessContext<TMessage, TResult> MessageProcessContext { get; private set; }

        public ProcessingContext(string processName, TMessage message, IMessageProcessContext<TMessage, TResult> messageProcessContext)
        {
            ProcessName = processName;
            Message = message;
            MessageProcessContext = messageProcessContext;
        }

        public abstract bool Process();
        public virtual bool ProcessCallback(object obj)
        {
            MessageProcessContext.OnMessageProcessed(Message, default(TResult));
            return true;
        }
    }
}
