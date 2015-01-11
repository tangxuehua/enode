using System;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Messaging.Impl
{
    public class DefaultMessageProcessor : AbstractParallelProcessor<IMessage>
    {
        #region Private Variables

        private readonly ITypeCodeProvider<IMessageHandler> _messageHandlerTypeCodeProvider;
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;
        private readonly IHandlerProvider<IMessageHandler> _messageHandlerProvider;
        private readonly ICommandService _commandService;
        private readonly IRepository _repository;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultMessageProcessor(
            ITypeCodeProvider<IMessageHandler> messageHandlerTypeCodeProvider,
            ITypeCodeProvider<ICommand> commandTypeCodeProvider,
            IHandlerProvider<IMessageHandler> messageHandlerProvider,
            ICommandService commandService,
            IRepository repository,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
            : base(ENodeConfiguration.Instance.Setting.MessageProcessorParallelThreadCount, "ProcessMessage")
        {
            Name = GetType().Name;
            _messageHandlerTypeCodeProvider = messageHandlerTypeCodeProvider;
            _commandTypeCodeProvider = commandTypeCodeProvider;
            _messageHandlerProvider = messageHandlerProvider;
            _commandService = commandService;
            _repository = repository;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        protected override QueueMessage<IMessage> CreateQueueMessage(IMessage message, IProcessContext<IMessage> processContext)
        {
            var hashKey = message is IVersionedMessage ? ((IVersionedMessage)message).SourceId : message.Id;
            return new QueueMessage<IMessage>(hashKey, message, processContext);
        }
        protected override void HandleQueueMessage(QueueMessage<IMessage> queueMessage)
        {
            var success = true;
            var message = queueMessage.Payload;

            foreach (var messageHandler in _messageHandlerProvider.GetHandlers(message.GetType()))
            {
                if (!DispatchMessageToHandler(message, messageHandler))
                {
                    success = false;
                }
            }

            if (!success)
            {
                _logger.ErrorFormat("Process message failed, messageId:{0}, messageType:{1}, retryTimes:{2}", message.Id, message.GetType().Name, queueMessage.RetryTimes);
            }

            OnMessageHandled(success, queueMessage);
        }

        #region Private Methods

        private bool DispatchMessageToHandler(IMessage message, IMessageHandler messageHandler)
        {
            var handlerType = messageHandler.GetInnerHandler().GetType();
            var handlerTypeCode = _messageHandlerTypeCodeProvider.GetTypeCode(handlerType);
            var handlingContext = new DefaultHandlingContext(_repository);

            try
            {
                messageHandler.Handle(handlingContext, message);
                var commands = handlingContext.GetCommands();
                if (commands.Any())
                {
                    foreach (var command in commands)
                    {
                        command.Id = BuildCommandId(command, message, handlerTypeCode);
                        _commandService.Send(command, message.Id, "Message");
                        _logger.DebugFormat("Send command success, commandType:{0}, commandId:{1}, messageHandlerType:{2}, messageType:{3}, messageId:{4}",
                            command.GetType().Name,
                            command.Id,
                            handlerType.Name,
                            message.GetType().Name,
                            message.Id);
                    }
                }
                _logger.DebugFormat("Handle message success. messageId:{0}, messageType:{1}, handlerType:{2}",
                    message.Id,
                    message.GetType().Name,
                    handlerType.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when [{0}] handling [{1}].", handlerType.Name, message.GetType().Name), ex);
                return false;
            }
        }
        private string BuildCommandId(ICommand command, IMessage message, int messageHandlerTypeCode)
        {
            var key = command.GetKey();
            var commandKey = key == null ? string.Empty : key.ToString();
            var commandTypeCode = _commandTypeCodeProvider.GetTypeCode(command.GetType());
            return string.Format("{0}{1}{2}{3}", message.Id, commandKey, messageHandlerTypeCode, commandTypeCode);
        }

        #endregion
    }
}
