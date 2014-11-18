namespace ENode.Infrastructure.Impl
{
    public abstract class ProcessingContext<TMessage> : IProcessingContext where TMessage : class
    {
        public string Name { get; private set; }
        public TMessage Message { get; private set; }
        public IProcessContext<TMessage> MessageProcessContext { get; private set; }

        public ProcessingContext(string processName, TMessage message, IProcessContext<TMessage> messageProcessContext)
        {
            Name = processName;
            Message = message;
            MessageProcessContext = messageProcessContext;
        }

        public abstract bool Process();
        public bool Callback(object obj)
        {
            OnMessageProcessed();
            return true;
        }
        protected virtual void OnMessageProcessed()
        {
            MessageProcessContext.OnProcessed(Message);
        }
    }
}
