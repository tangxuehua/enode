using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommon.Extensions;
using ECommon.Logging;

namespace ENode.Infrastructure.Impl
{
    public class DefaultMessageDispatcher : IMessageDispatcher
    {
        #region Private Variables

        private readonly ITypeCodeProvider _typeCodeProvider;
        private readonly IMessageHandlerProvider _handlerProvider;
        private readonly IMessageHandleRecordStore _messageHandleRecordStore;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultMessageDispatcher(
            ITypeCodeProvider typeCodeProvider,
            IMessageHandlerProvider handlerProvider,
            IMessageHandleRecordStore messageHandleRecordStore,
            IOHelper ioHelper,
            ILoggerFactory loggerFactory)
        {
            _typeCodeProvider = typeCodeProvider;
            _handlerProvider = handlerProvider;
            _messageHandleRecordStore = messageHandleRecordStore;
            _ioHelper = ioHelper;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        public Task<AsyncTaskResult> DispatchMessageAsync(IMessage message)
        {
            return DispatchMessagesAsync(new List<IMessage> { message });
        }
        public Task<AsyncTaskResult> DispatchMessagesAsync(IEnumerable<IMessage> messages)
        {
            var messageStream = new DisptachingMessageStream(messages);
            foreach (var message in messages)
            {
                var handlers = _handlerProvider.GetHandlers(message.GetType());
                if (!handlers.Any())
                {
                    messageStream.RemoveHandledMessage(message.Id);
                    continue;
                }
                var dispatchingMessage = new DisptachingMessage(message, messageStream, handlers);
                foreach (var handler in handlers)
                {
                    DispatchMessageToHandlerAsync(dispatchingMessage, handler, 0);
                }
            }
            return messageStream.Task;
        }

        private void DispatchMessageToHandlerAsync(DisptachingMessage dispatchingMessage, IMessageHandlerProxy handlerProxy, int retryTimes)
        {
            var message = dispatchingMessage.Message;
            var messageTypeCode = _typeCodeProvider.GetTypeCode(message.GetType());
            var handlerType = handlerProxy.GetInnerHandler().GetType();
            var handlerTypeCode = _typeCodeProvider.GetTypeCode(handlerType);
            var aggregateRootTypeCode = message is ISequenceMessage ? ((ISequenceMessage)message).AggregateRootTypeCode : 0;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<bool>>("IsRecordExistAsync",
            () => _messageHandleRecordStore.IsRecordExistAsync(message.Id, handlerTypeCode, aggregateRootTypeCode),
            currentRetryTimes => DispatchMessageToHandlerAsync(dispatchingMessage, handlerProxy, currentRetryTimes),
            result =>
            {
                if (result.Data)
                {
                    dispatchingMessage.RemoveHandledHandler(handlerType);
                }
                else
                {
                    HandleMessageAsync(dispatchingMessage, handlerProxy, handlerTypeCode, messageTypeCode, 0);
                }
            },
            () => string.Format("[messageId:{0}, messageType:{1}, handlerType:{2}]", message.Id, message.GetType().Name, handlerType.Name),
            null,
            retryTimes,
            true);
        }
        private void HandleMessageAsync(DisptachingMessage dispatchingMessage, IMessageHandlerProxy handlerProxy, int handlerTypeCode, int messageTypeCode, int retryTimes)
        {
            var message = dispatchingMessage.Message;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult>("HandleMessageAsync",
            () => handlerProxy.HandleAsync(message),
            currentRetryTimes => HandleMessageAsync(dispatchingMessage, handlerProxy, handlerTypeCode, messageTypeCode, currentRetryTimes),
            result =>
            {
                var messageHandleRecord = new MessageHandleRecord
                {
                    MessageId = message.Id,
                    HandlerTypeCode = handlerTypeCode,
                    MessageTypeCode = messageTypeCode,
                    Timestamp = DateTime.Now
                };
                var sequenceMessage = message as ISequenceMessage;
                if (sequenceMessage != null)
                {
                    messageHandleRecord.AggregateRootTypeCode = sequenceMessage.AggregateRootTypeCode;
                    messageHandleRecord.AggregateRootId = sequenceMessage.AggregateRootId;
                    messageHandleRecord.Version = sequenceMessage.Version;
                }

                AddMessageHandledRecordAsync(dispatchingMessage, messageHandleRecord, handlerProxy.GetInnerHandler().GetType(), 0);
            },
            () => string.Format("[messageId:{0}, messageType:{1}, handlerType:{2}]", message.Id, message.GetType().Name, handlerProxy.GetInnerHandler().GetType().Name),
            null,
            retryTimes,
            true);
        }
        private void AddMessageHandledRecordAsync(DisptachingMessage dispatchingMessage, MessageHandleRecord messageHandleRecord, Type handlerType, int retryTimes)
        {
            var message = dispatchingMessage.Message;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult>("AddMessageHandledRecordAsync",
            () => _messageHandleRecordStore.AddRecordAsync(messageHandleRecord),
            currentRetryTimes => AddMessageHandledRecordAsync(dispatchingMessage, messageHandleRecord, handlerType, currentRetryTimes),
            result =>
            {
                dispatchingMessage.RemoveHandledHandler(handlerType);
                _logger.DebugFormat("Message handled success, handlerType:{0}, messageType:{1}, messageId:{2}", handlerType.Name, message.GetType().Name, message.Id);
            },
            () => string.Format("[messageId:{0}, messageType:{1}, handlerType:{2}]", message.Id, message.GetType().Name, handlerType.Name),
            null,
            retryTimes,
            true);
        }

        class DisptachingMessageStream
        {
            private TaskCompletionSource<AsyncTaskResult> _taskCompletionSource;
            private ConcurrentDictionary<string, IMessage> _messageDict;

            public Task<AsyncTaskResult> Task { get { return _taskCompletionSource.Task; } }

            public DisptachingMessageStream(IEnumerable<IMessage> messages)
            {
                _taskCompletionSource = new TaskCompletionSource<AsyncTaskResult>();
                _messageDict = new ConcurrentDictionary<string, IMessage>();
                messages.ForEach(x => _messageDict.TryAdd(x.Id, x));
            }

            public void RemoveHandledMessage(string messageId)
            {
                IMessage message;
                if (_messageDict.TryRemove(messageId, out message))
                {
                    if (_messageDict.IsEmpty)
                    {
                        _taskCompletionSource.SetResult(AsyncTaskResult.Success);
                    }
                }
            }
        }
        class DisptachingMessage
        {
            private ConcurrentDictionary<Type, IMessageHandlerProxy> _handlerDict;
            private DisptachingMessageStream _parentMessageStream;

            public IMessage Message { get; private set; }

            public DisptachingMessage(IMessage message, DisptachingMessageStream parentMessageStream, IEnumerable<IMessageHandlerProxy> handlers)
            {
                Message = message;
                _parentMessageStream = parentMessageStream;
                _handlerDict = new ConcurrentDictionary<Type, IMessageHandlerProxy>();
                handlers.ForEach(x => _handlerDict.TryAdd(x.GetInnerHandler().GetType(), x));
            }

            public void RemoveHandledHandler(Type handlerType)
            {
                IMessageHandlerProxy handler;
                if (_handlerDict.TryRemove(handlerType, out handler))
                {
                    if (_handlerDict.IsEmpty)
                    {
                        _parentMessageStream.RemoveHandledMessage(Message.Id);
                    }
                }
            }
        }
    }
}
