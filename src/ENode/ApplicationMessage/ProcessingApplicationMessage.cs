namespace ENode.Infrastructure
{
    public class ProcessingApplicationMessage
    {
        private IMessageProcessContext _processContext;

        public IApplicationMessage Message { get; private set; }

        public ProcessingApplicationMessage(IApplicationMessage message, IMessageProcessContext processContext)
        {
            Message = message;
            _processContext = processContext;
        }

        public void Complete()
        {
            _processContext.NotifyMessageProcessed();
        }
    }
}
