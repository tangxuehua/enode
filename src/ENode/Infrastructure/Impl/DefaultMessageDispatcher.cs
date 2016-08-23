using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommon.Extensions;
using ECommon.IO;
using ECommon.Logging;

namespace ENode.Infrastructure.Impl
{
    public class DefaultMessageDispatcher : IMessageDispatcher
    {
        #region Private Variables

        private readonly ITypeNameProvider _typeNameProvider;
        private readonly IMessageHandlerProvider _handlerProvider;
        private readonly ITwoMessageHandlerProvider _twoMessageHandlerProvider;
        private readonly IThreeMessageHandlerProvider _threeMessageHandlerProvider;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultMessageDispatcher(
            ITypeNameProvider typeNameProvider,
            IMessageHandlerProvider handlerProvider,
            ITwoMessageHandlerProvider twoMessageHandlerProvider,
            IThreeMessageHandlerProvider threeMessageHandlerProvider,
            IOHelper ioHelper,
            ILoggerFactory loggerFactory)
        {
            _typeNameProvider = typeNameProvider;
            _handlerProvider = handlerProvider;
            _twoMessageHandlerProvider = twoMessageHandlerProvider;
            _threeMessageHandlerProvider = threeMessageHandlerProvider;
            _ioHelper = ioHelper;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        public Task<AsyncTaskResult> DispatchMessageAsync(IMessage message)
        {
            return DispatchMessages(new List<IMessage> { message });
        }
        public Task<AsyncTaskResult> DispatchMessagesAsync(IEnumerable<IMessage> messages)
        {
            return DispatchMessages(messages);
        }

        #region Private Methods

        private Task<AsyncTaskResult> DispatchMessages(IEnumerable<IMessage> messages)
        {
            var messageCount = messages.Count();
            if (messageCount == 0)
            {
                return Task.FromResult<AsyncTaskResult>(AsyncTaskResult.Success);
            }
            var rootDispatching = new RootDisptaching();

            //先对每个事件调用其Handler
            var queueMessageDispatching = new QueueMessageDisptaching(this, rootDispatching, messages);
            DispatchSingleMessage(queueMessageDispatching.DequeueMessage(), queueMessageDispatching);

            //如果有至少两个事件，则尝试调用针对两个事件的Handler
            if (messageCount >= 2)
            {
                var twoMessageHandlers = _twoMessageHandlerProvider.GetHandlers(messages.Select(x => x.GetType()));
                if (twoMessageHandlers.IsNotEmpty())
                {
                    DispatchMultiMessage(messages, twoMessageHandlers, rootDispatching, DispatchTwoMessageToHandlerAsync);
                }
            }
            //如果有至少三个事件，则尝试调用针对三个事件的Handler
            if (messageCount >= 3)
            {
                var threeMessageHandlers = _threeMessageHandlerProvider.GetHandlers(messages.Select(x => x.GetType()));
                if (threeMessageHandlers.IsNotEmpty())
                {
                    DispatchMultiMessage(messages, threeMessageHandlers, rootDispatching, DispatchThreeMessageToHandlerAsync);
                }
            }
            return rootDispatching.Task;
        }

        private void DispatchSingleMessage(IMessage message, QueueMessageDisptaching queueMessageDispatching)
        {
            var messageHandlerDataList = _handlerProvider.GetHandlers(message.GetType());
            if (!messageHandlerDataList.Any())
            {
                queueMessageDispatching.OnMessageHandled(message);
                return;
            }

            foreach (var messageHandlerData in messageHandlerDataList)
            {
                var singleMessageDispatching = new SingleMessageDisptaching(message, queueMessageDispatching, messageHandlerData.AllHandlers, _typeNameProvider);

                if (messageHandlerData.ListHandlers != null && messageHandlerData.ListHandlers.IsNotEmpty())
                {
                    foreach (var handler in messageHandlerData.ListHandlers)
                    {
                        DispatchSingleMessageToHandlerAsync(singleMessageDispatching, handler, null, 0);
                    }
                }
                if (messageHandlerData.QueuedHandlers != null && messageHandlerData.QueuedHandlers.IsNotEmpty())
                {
                    var queueHandler = new QueuedHandler<IMessageHandlerProxy1>(messageHandlerData.QueuedHandlers, (queuedHandler, nextHandler) => DispatchSingleMessageToHandlerAsync(singleMessageDispatching, nextHandler, queuedHandler, 0));
                    DispatchSingleMessageToHandlerAsync(singleMessageDispatching, queueHandler.DequeueHandler(), queueHandler, 0);
                }
            }
        }
        private void DispatchMultiMessage<T>(IEnumerable<IMessage> messages, IEnumerable<MessageHandlerData<T>> messageHandlerDataList, RootDisptaching rootDispatching, Action<MultiMessageDisptaching, T, QueuedHandler<T>, int> dispatchAction) where T : class, IObjectProxy
        {
            foreach (var messageHandlerData in messageHandlerDataList)
            {
                var multiMessageDispatching = new MultiMessageDisptaching(messages, messageHandlerData.AllHandlers, rootDispatching, _typeNameProvider);

                if (messageHandlerData.ListHandlers != null && messageHandlerData.ListHandlers.IsNotEmpty())
                {
                    foreach (var handler in messageHandlerData.ListHandlers)
                    {
                        dispatchAction(multiMessageDispatching, handler, null, 0);
                    }
                }
                if (messageHandlerData.QueuedHandlers != null && messageHandlerData.QueuedHandlers.IsNotEmpty())
                {
                    var queuedHandler = new QueuedHandler<T>(messageHandlerData.QueuedHandlers,
                        (currentQueuedHandler, nextHandler) => dispatchAction(multiMessageDispatching, nextHandler, currentQueuedHandler, 0));
                    dispatchAction(multiMessageDispatching, queuedHandler.DequeueHandler(), queuedHandler, 0);
                }
            }
        }

        private void DispatchSingleMessageToHandlerAsync(SingleMessageDisptaching singleMessageDispatching, IMessageHandlerProxy1 handlerProxy, QueuedHandler<IMessageHandlerProxy1> queueHandler, int retryTimes)
        {
            var message = singleMessageDispatching.Message;
            var messageTypeName = _typeNameProvider.GetTypeName(message.GetType());
            var handlerType = handlerProxy.GetInnerObject().GetType();
            var handlerTypeName = _typeNameProvider.GetTypeName(handlerType);
            HandleSingleMessageAsync(singleMessageDispatching, handlerProxy, handlerTypeName, messageTypeName, queueHandler, 0);
        }
        private void DispatchTwoMessageToHandlerAsync(MultiMessageDisptaching multiMessageDispatching, IMessageHandlerProxy2 handlerProxy, QueuedHandler<IMessageHandlerProxy2> queueHandler, int retryTimes)
        {
            var handlerType = handlerProxy.GetInnerObject().GetType();
            var handlerTypeName = _typeNameProvider.GetTypeName(handlerType);
            HandleTwoMessageAsync(multiMessageDispatching, handlerProxy, handlerTypeName, queueHandler, 0);
        }
        private void DispatchThreeMessageToHandlerAsync(MultiMessageDisptaching multiMessageDispatching, IMessageHandlerProxy3 handlerProxy, QueuedHandler<IMessageHandlerProxy3> queueHandler, int retryTimes)
        {
            var handlerType = handlerProxy.GetInnerObject().GetType();
            var handlerTypeName = _typeNameProvider.GetTypeName(handlerType);
            HandleThreeMessageAsync(multiMessageDispatching, handlerProxy, handlerTypeName, queueHandler, 0);
        }

        private void HandleSingleMessageAsync(SingleMessageDisptaching singleMessageDispatching, IMessageHandlerProxy1 handlerProxy, string handlerTypeName, string messageTypeName, QueuedHandler<IMessageHandlerProxy1> queueHandler, int retryTimes)
        {
            var message = singleMessageDispatching.Message;

            _ioHelper.TryAsyncActionRecursively("HandleSingleMessageAsync",
            () => handlerProxy.HandleAsync(message),
            currentRetryTimes => HandleSingleMessageAsync(singleMessageDispatching, handlerProxy, handlerTypeName, messageTypeName, queueHandler, currentRetryTimes),
            result =>
            {
                singleMessageDispatching.RemoveHandledHandler(handlerTypeName);
                if (queueHandler != null)
                {
                    queueHandler.OnHandlerFinished(handlerProxy);
                }
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("Message handled success, handlerType:{0}, messageType:{1}, messageId:{2}", handlerTypeName, message.GetType().Name, message.Id);
                }
            },
            () => string.Format("[messageId:{0}, messageType:{1}, handlerType:{2}]", message.Id, message.GetType().Name, handlerProxy.GetInnerObject().GetType().Name),
            errorMessage =>
            {
                _logger.Fatal(string.Format("Handle single message has unknown exception, the code should not be run to here, errorMessage: {0}", errorMessage));
            },
            retryTimes, true);
        }
        private void HandleTwoMessageAsync(MultiMessageDisptaching multiMessageDispatching, IMessageHandlerProxy2 handlerProxy, string handlerTypeName, QueuedHandler<IMessageHandlerProxy2> queueHandler, int retryTimes)
        {
            var messages = multiMessageDispatching.Messages;
            var message1 = messages[0];
            var message2 = messages[1];

            _ioHelper.TryAsyncActionRecursively("HandleTwoMessageAsync",
            () => handlerProxy.HandleAsync(message1, message2),
            currentRetryTimes => HandleTwoMessageAsync(multiMessageDispatching, handlerProxy, handlerTypeName, queueHandler, currentRetryTimes),
            result =>
            {
                multiMessageDispatching.RemoveHandledHandler(handlerTypeName);
                if (queueHandler != null)
                {
                    queueHandler.OnHandlerFinished(handlerProxy);
                }
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("TwoMessage handled success, [messages:[{0}], handlerType:{1}]", string.Join("|", messages.Select(x => string.Format("id:{0},type:{1}", x.Id, x.GetType().Name))), handlerTypeName);
                }
            },
            () => string.Format("[messages:[{0}], handlerType:{1}]", string.Join("|", messages.Select(x => string.Format("id:{0},type:{1}", x.Id, x.GetType().Name))), handlerProxy.GetInnerObject().GetType().Name),
            errorMessage =>
            {
                _logger.Fatal(string.Format("Handle two message has unknown exception, the code should not be run to here, errorMessage: {0}", errorMessage));
            },
            retryTimes, true);
        }
        private void HandleThreeMessageAsync(MultiMessageDisptaching multiMessageDispatching, IMessageHandlerProxy3 handlerProxy, string handlerTypeName, QueuedHandler<IMessageHandlerProxy3> queueHandler, int retryTimes)
        {
            var messages = multiMessageDispatching.Messages;
            var message1 = messages[0];
            var message2 = messages[1];
            var message3 = messages[2];

            _ioHelper.TryAsyncActionRecursively("HandleThreeMessageAsync",
            () => handlerProxy.HandleAsync(message1, message2, message3),
            currentRetryTimes => HandleThreeMessageAsync(multiMessageDispatching, handlerProxy, handlerTypeName, queueHandler, currentRetryTimes),
            result =>
            {
                multiMessageDispatching.RemoveHandledHandler(handlerTypeName);
                if (queueHandler != null)
                {
                    queueHandler.OnHandlerFinished(handlerProxy);
                }
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("ThreeMessage handled success, [messages:[{0}], handlerType:{1}]", string.Join("|", messages.Select(x => string.Format("id:{0},type:{1}", x.Id, x.GetType().Name))), handlerTypeName);
                }
            },
            () => string.Format("[messages:[{0}], handlerType:{1}]", string.Join("|", messages.Select(x => string.Format("id:{0},type:{1}", x.Id, x.GetType().Name))), handlerProxy.GetInnerObject().GetType().Name),
            errorMessage =>
            {
                _logger.Fatal(string.Format("Handle three message has unknown exception, the code should not be run to here, errorMessage: {0}", errorMessage));
            },
            retryTimes, true);
        }

        #endregion

        #region Private Classes

        class RootDisptaching
        {
            private TaskCompletionSource<AsyncTaskResult> _taskCompletionSource;
            private ConcurrentDictionary<object, bool> _childDispatchingDict;

            public Task<AsyncTaskResult> Task
            {
                get { return _taskCompletionSource.Task; }
            }

            public RootDisptaching()
            {
                _taskCompletionSource = new TaskCompletionSource<AsyncTaskResult>();
                _childDispatchingDict = new ConcurrentDictionary<object, bool>();
            }

            public void AddChildDispatching(object childDispatching)
            {
                _childDispatchingDict.TryAdd(childDispatching, false);
            }
            public void OnChildDispatchingFinished(object childDispatching)
            {
                bool removed;
                if (_childDispatchingDict.TryRemove(childDispatching, out removed))
                {
                    if (_childDispatchingDict.IsEmpty)
                    {
                        _taskCompletionSource.SetResult(AsyncTaskResult.Success);
                    }
                }
            }
        }
        class QueueMessageDisptaching
        {
            private DefaultMessageDispatcher _dispatcher;
            private RootDisptaching _rootDispatching;
            private ConcurrentQueue<IMessage> _messageQueue;

            public QueueMessageDisptaching(DefaultMessageDispatcher dispatcher, RootDisptaching rootDispatching, IEnumerable<IMessage> messages)
            {
                _dispatcher = dispatcher;
                _messageQueue = new ConcurrentQueue<IMessage>();
                messages.ForEach(message => _messageQueue.Enqueue(message));
                _rootDispatching = rootDispatching;
                _rootDispatching.AddChildDispatching(this);
            }

            public IMessage DequeueMessage()
            {
                IMessage message;
                if (_messageQueue.TryDequeue(out message))
                {
                    return message;
                }
                return null;
            }
            public void OnMessageHandled(IMessage message)
            {
                var nextMessage = DequeueMessage();
                if (nextMessage == null)
                {
                    _rootDispatching.OnChildDispatchingFinished(this);
                    return;
                }
                _dispatcher.DispatchSingleMessage(nextMessage, this);
            }
        }
        class MultiMessageDisptaching
        {
            private IMessage[] _messages;
            private ConcurrentDictionary<string, IObjectProxy> _handlerDict;
            private RootDisptaching _rootDispatching;

            public IMessage[] Messages
            {
                get { return _messages; }
            }

            public MultiMessageDisptaching(IEnumerable<IMessage> messages, IEnumerable<IObjectProxy> handlers, RootDisptaching rootDispatching, ITypeNameProvider typeNameProvider)
            {
                _messages = messages.ToArray();
                _handlerDict = new ConcurrentDictionary<string, IObjectProxy>();
                handlers.ForEach(x => _handlerDict.TryAdd(typeNameProvider.GetTypeName(x.GetInnerObject().GetType()), x));
                _rootDispatching = rootDispatching;
                _rootDispatching.AddChildDispatching(this);
            }

            public void RemoveHandledHandler(string handlerTypeName)
            {
                IObjectProxy handler;
                if (_handlerDict.TryRemove(handlerTypeName, out handler))
                {
                    if (_handlerDict.IsEmpty)
                    {
                        _rootDispatching.OnChildDispatchingFinished(this);
                    }
                }
            }
        }
        class SingleMessageDisptaching
        {
            private ConcurrentDictionary<string, IObjectProxy> _handlerDict;
            private QueueMessageDisptaching _queueMessageDispatching;

            public IMessage Message { get; private set; }

            public SingleMessageDisptaching(IMessage message, QueueMessageDisptaching queueMessageDispatching, IEnumerable<IObjectProxy> handlers, ITypeNameProvider typeNameProvider)
            {
                Message = message;
                _queueMessageDispatching = queueMessageDispatching;
                _handlerDict = new ConcurrentDictionary<string, IObjectProxy>();
                handlers.ForEach(x => _handlerDict.TryAdd(typeNameProvider.GetTypeName(x.GetInnerObject().GetType()), x));
            }

            public void RemoveHandledHandler(string handlerTypeName)
            {
                IObjectProxy handler;
                if (_handlerDict.TryRemove(handlerTypeName, out handler))
                {
                    if (_handlerDict.IsEmpty)
                    {
                        _queueMessageDispatching.OnMessageHandled(Message);
                    }
                }
            }
        }
        class QueuedHandler<T> where T : class, IObjectProxy
        {
            private Action<QueuedHandler<T>, T> _dispatchToNextHandler;
            private ConcurrentQueue<T> _handlerQueue;

            public QueuedHandler(IEnumerable<T> handlers, Action<QueuedHandler<T>, T> dispatchToNextHandler)
            {
                _handlerQueue = new ConcurrentQueue<T>();
                handlers.ForEach(handler => _handlerQueue.Enqueue(handler));
                _dispatchToNextHandler = dispatchToNextHandler;
            }

            public T DequeueHandler()
            {
                T handler;
                if (_handlerQueue.TryDequeue(out handler))
                {
                    return handler;
                }
                return null;
            }
            public void OnHandlerFinished(T handler)
            {
                var nextHandler = DequeueHandler();
                if (nextHandler != null)
                {
                    _dispatchToNextHandler(this, nextHandler);
                }
            }
        }

        #endregion
    }
}
