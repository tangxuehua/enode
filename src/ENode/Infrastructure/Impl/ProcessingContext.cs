namespace ENode.Infrastructure.Impl
{
    public abstract class ProcessingContext<TMessage> : IProcessingContext where TMessage : class
    {
        public string ProcessName { get; private set; }
        public TMessage Message { get; private set; }
        public IMessageProcessContext<TMessage> MessageProcessContext { get; private set; }

        public ProcessingContext(string processName, TMessage message, IMessageProcessContext<TMessage> messageProcessContext)
        {
            ProcessName = processName;
            Message = message;
            MessageProcessContext = messageProcessContext;
        }

        public abstract bool Process();
        public bool ProcessCallback(object obj)
        {
            OnMessageProcessed();
            return true;
        }
        protected virtual void OnMessageProcessed()
        {
            MessageProcessContext.OnMessageProcessed(Message);
        }
    }
}
