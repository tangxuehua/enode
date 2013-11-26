using System;

namespace ENode.Messaging.Impl
{
    /// <summary>The default implementation of IProcessedMessageMessageHandler.
    /// </summary>
    public class DefaultProcessedMessageMessageHandler : MessageHandler<IMessage>, IProcessedMessageMessageHandler
    {
        public override void Handle(Message<IMessage> message)
        {
            //try
            //{
            //    //TODO
            //    //Remove the message from the queue.
            //}
            //catch (Exception ex)
            //{
            //    //_logger.Error(string.Format("Exception raised when removing queue message:{0} from queue {1}.", processedMessage.Message.Id, processedMessage.QueueName), ex);
            //    //_processedMessageCache.AddMessage(processedMessage);
            //}
        }
    }
}