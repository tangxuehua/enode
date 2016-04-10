namespace ENode.Infrastructure.Impl
{
    public class DefaultProcessingMessageHandler<X, Y, Z> : IProcessingMessageHandler<X, Y, Z>
        where X : class, IProcessingMessage<X, Y, Z>
        where Y : IMessage
    {
        private readonly IMessageDispatcher _dispatcher;

        public DefaultProcessingMessageHandler(IMessageDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async void HandleAsync(X processingMessage)
        {
            await _dispatcher.DispatchMessageAsync(processingMessage.Message);
            processingMessage.SetResult(default(Z));
        }
    }
}
