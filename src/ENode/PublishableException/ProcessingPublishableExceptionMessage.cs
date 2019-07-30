namespace ENode.Infrastructure
{
    public class ProcessingPublishableExceptionMessage
    {
        private IMessageProcessContext _processContext;

        public IPublishableException Message { get; private set; }

        public ProcessingPublishableExceptionMessage(IPublishableException message, IMessageProcessContext processContext)
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
