using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ENode.Commanding;
using ENode.Domain;

namespace ENode.Infrastructure.Impl
{
    public abstract class AbstractDispatcher<TMessage, TMessageHandler> : IDispatcher<TMessage>
        where TMessage : class, IDispatchableMessage
        where TMessageHandler : class, IProxyHandler
    {
        #region Private Variables

        private readonly ITypeCodeProvider<TMessage> _messageTypeCodeProvider;
        private readonly ITypeCodeProvider<TMessageHandler> _handlerTypeCodeProvider;
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;
        private readonly IHandlerProvider<TMessageHandler> _handlerProvider;
        private readonly ICommandService _commandService;
        private readonly IRepository _repository;
        private readonly IMessageHandleRecordStore _messageHandleRecordStore;
        private readonly IMessageHandleRecordCache _messageHandleRecordCache;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public AbstractDispatcher(
            ITypeCodeProvider<TMessage> messageTypeCodeProvider,
            ITypeCodeProvider<TMessageHandler> handlerTypeCodeProvider,
            ITypeCodeProvider<ICommand> commandTypeCodeProvider,
            IHandlerProvider<TMessageHandler> handlerProvider,
            ICommandService commandService,
            IRepository repository,
            IMessageHandleRecordStore messageHandleRecordStore,
            IMessageHandleRecordCache messageHandleRecordCache,
            IOHelper ioHelper,
            ILoggerFactory loggerFactory)
        {
            _messageTypeCodeProvider = messageTypeCodeProvider;
            _handlerTypeCodeProvider = handlerTypeCodeProvider;
            _commandTypeCodeProvider = commandTypeCodeProvider;
            _handlerProvider = handlerProvider;
            _commandService = commandService;
            _repository = repository;
            _messageHandleRecordStore = messageHandleRecordStore;
            _messageHandleRecordCache = messageHandleRecordCache;
            _ioHelper = ioHelper;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        public bool DispatchMessage(TMessage message)
        {
            return DispatchMessageToHandlers(message, true);
        }
        public bool DispatchMessages(IEnumerable<TMessage> messages)
        {
            var success = true;
            foreach (var message in messages)
            {
                if (!DispatchMessageToHandlers(message, false))
                {
                    success = false;
                }
            }
            if (success)
            {
                foreach (var message in messages)
                {
                    _messageHandleRecordCache.RemoveRecordFromCache(GetHandleRecordType(message), message.Id);
                }
            }
            return success;
        }

        protected abstract MessageHandleRecordType GetHandleRecordType(TMessage message);
        protected abstract void HandleMessage(TMessage message, TMessageHandler messageHandler, IHandlingContext handlingContext);
        protected virtual void OnMessageHandleRecordCreated(TMessage message, MessageHandleRecord record) { }

        private bool DispatchMessageToHandlers(TMessage message, bool autoRemoveMessageHandleRecordCache)
        {
            var success = true;
            foreach (var handler in _handlerProvider.GetHandlers(message.GetType()))
            {
                var result = _ioHelper.TryIOFuncRecursively<bool>("DispatchMessageToHandler", () =>
                {
                    return string.Format("[messageType:{0},messageId:{1},handlerType:{2}]", message.GetType().Name, message.Id, handler.GetInnerHandler().GetType().Name);
                }, () =>
                {
                    return DispatchMessageToHandler(message, handler);
                });
                if (!result.Data)
                {
                    success = false;
                }
            }
            if (autoRemoveMessageHandleRecordCache)
            {
                _messageHandleRecordCache.RemoveRecordFromCache(GetHandleRecordType(message), message.Id);
            }
            return success;
        }
        private bool DispatchMessageToHandler(TMessage message, TMessageHandler messageHandler)
        {
            var messageTypeCode = _messageTypeCodeProvider.GetTypeCode(message.GetType());
            var handlerType = messageHandler.GetInnerHandler().GetType();
            var handlerTypeCode = _handlerTypeCodeProvider.GetTypeCode(handlerType);
            var handleRecordType = GetHandleRecordType(message);
            if (_messageHandleRecordCache.IsRecordExist(handleRecordType, message.Id, handlerTypeCode)) return true;
            if (_messageHandleRecordStore.IsRecordExist(handleRecordType, message.Id, handlerTypeCode)) return true;
            var handlingContext = new DefaultHandlingContext(_repository);

            try
            {
                HandleMessage(message, messageHandler, handlingContext);
                var commands = handlingContext.GetCommands();
                if (commands.Any())
                {
                    foreach (var command in commands)
                    {
                        command.Id = BuildCommandId(command, message, handlerTypeCode);
                        _commandService.Send(command, message.Id, handleRecordType.ToString());
                        _logger.DebugFormat("Send command success, commandType:{0}, commandId:{1}, handlerType:{2}, messageType:{3}, messageId:{4}",
                            command.GetType().Name,
                            command.Id,
                            handlerType.Name,
                            message.GetType().Name,
                            message.Id);
                    }
                }

                var messageHandleRecord = new MessageHandleRecord
                {
                    Type = handleRecordType,
                    MessageId = message.Id,
                    HandlerTypeCode = handlerTypeCode,
                    MessageTypeCode = messageTypeCode
                };
                OnMessageHandleRecordCreated(message, messageHandleRecord);

                _messageHandleRecordStore.AddRecord(messageHandleRecord);
                _messageHandleRecordCache.AddRecord(messageHandleRecord);

                _logger.DebugFormat("Message handle success, handlerType:{0}, messageType:{1}, messageId:{2}",
                    handlerType.Name,
                    message.GetType().Name,
                    message.Id);

                return true;
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Message handle failed, handlerType:{0}, messageType:{1}, messageId:{2}.",
                    handlerType.Name,
                    message.GetType().Name,
                    message.Id), ex);
                return !(ex is ICanBeRetryException);
            }
        }
        private string BuildCommandId(ICommand command, TMessage message, int handlerTypeCode)
        {
            var key = command.GetKey();
            var commandKey = key == null ? string.Empty : key.ToString();
            var commandTypeCode = _commandTypeCodeProvider.GetTypeCode(command.GetType());
            return string.Format("{0}{1}{2}{3}", message.Id, commandKey, handlerTypeCode, commandTypeCode);
        }
    }
}
