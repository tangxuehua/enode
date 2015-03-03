using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ECommon.Extensions;
using ECommon.Logging;
using ENode.Commanding;
using ENode.Domain;

namespace ENode.Infrastructure.Impl
{
    public abstract class AbstractDispatcher<TMessage> : IDispatcher<TMessage> where TMessage : class, IDispatchableMessage
    {
        #region Private Variables

        private readonly ITypeCodeProvider _typeCodeProvider;
        private readonly IHandlerProvider _handlerProvider;
        private readonly ICommandService _commandService;
        private readonly IRepository _repository;
        private readonly IMessageHandleRecordStore _messageHandleRecordStore;
        private readonly ConcurrentDictionary<string, ISet<int>> _messageHandleRecordCache;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public AbstractDispatcher(
            ITypeCodeProvider typeCodeProvider,
            IHandlerProvider handlerProvider,
            ICommandService commandService,
            IRepository repository,
            IMessageHandleRecordStore messageHandleRecordStore,
            IOHelper ioHelper,
            ILoggerFactory loggerFactory)
        {
            _typeCodeProvider = typeCodeProvider;
            _handlerProvider = handlerProvider;
            _commandService = commandService;
            _repository = repository;
            _messageHandleRecordStore = messageHandleRecordStore;
            _messageHandleRecordCache = new ConcurrentDictionary<string, ISet<int>>();
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
                    RemoveMessageHandleRecordFromCache(GetHandleRecordType(message), message.Id);
                }
            }
            return success;
        }

        protected abstract MessageHandleRecordType GetHandleRecordType(TMessage message);
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
                RemoveMessageHandleRecordFromCache(GetHandleRecordType(message), message.Id);
            }
            return success;
        }
        private bool DispatchMessageToHandler(TMessage message, IProxyHandler proxyHandler)
        {
            var messageTypeCode = _typeCodeProvider.GetTypeCode(message.GetType());
            var handlerType = proxyHandler.GetInnerHandler().GetType();
            var handlerTypeCode = _typeCodeProvider.GetTypeCode(handlerType);
            var handleRecordType = GetHandleRecordType(message);

            if (IsMessageHandleRecordExistInCache(handleRecordType, message.Id, handlerTypeCode)) return true;
            if (_messageHandleRecordStore.IsRecordExist(handleRecordType, message.Id, handlerTypeCode)) return true;

            try
            {
                proxyHandler.Handle(message);

                var messageHandleRecord = new MessageHandleRecord
                {
                    Type = handleRecordType,
                    MessageId = message.Id,
                    HandlerTypeCode = handlerTypeCode,
                    MessageTypeCode = messageTypeCode
                };
                OnMessageHandleRecordCreated(message, messageHandleRecord);

                _ioHelper.TryIOActionRecursively("AddMessageHandleRecord", () => messageHandleRecord.ToString(), () =>
                {
                    _messageHandleRecordStore.AddRecord(messageHandleRecord);
                });
                AddMessageHandleRecordToCache(messageHandleRecord);

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
        private void AddMessageHandleRecordToCache(MessageHandleRecord record)
        {
            _messageHandleRecordCache.GetOrAdd(record.MessageId, x => new HashSet<int>()).Add(record.HandlerTypeCode);
        }
        private bool IsMessageHandleRecordExistInCache(MessageHandleRecordType type, string messageId, int handlerTypeCode)
        {
            ISet<int> handlerTypeCodeList;
            return _messageHandleRecordCache.TryGetValue(messageId, out handlerTypeCodeList) && handlerTypeCodeList.Contains(handlerTypeCode);
        }
        private void RemoveMessageHandleRecordFromCache(MessageHandleRecordType type, string messageId)
        {
            _messageHandleRecordCache.Remove(messageId);
        }
    }
}
