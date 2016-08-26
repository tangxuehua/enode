using System;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;

namespace ENode.Infrastructure.Impl
{
    public class MessageHandlerProxy1<T> : IMessageHandlerProxy1 where T : class, IMessage
    {
        private IMessageHandler<T> _handler;
        private readonly Type _handlerType;

        public MessageHandlerProxy1(IMessageHandler<T> handler, Type handlerType)
        {
            _handler = handler;
            _handlerType = handlerType;
        }

        public Task<AsyncTaskResult> HandleAsync(IMessage message)
        {
            var handler = GetInnerObject() as IMessageHandler<T>;
            return handler.HandleAsync(message as T);
        }
        public object GetInnerObject()
        {
            if (_handler != null)
            {
                return _handler;
            }
            return ObjectContainer.Resolve(_handlerType);
        }
    }
    public class MessageHandlerProxy2<T1, T2> : IMessageHandlerProxy2
        where T1 : class, IMessage
        where T2 : class, IMessage
    {
        private IMessageHandler<T1, T2> _handler;
        private readonly Type _handlerType;

        public MessageHandlerProxy2(IMessageHandler<T1, T2> handler, Type handlerType)
        {
            _handler = handler;
            _handlerType = handlerType;
        }

        public Task<AsyncTaskResult> HandleAsync(IMessage message1, IMessage message2)
        {
            var handler = GetInnerObject() as IMessageHandler<T1, T2>;
            if (message1 is T1)
            {
                return handler.HandleAsync(message1 as T1, message2 as T2);
            }
            else
            {
                return handler.HandleAsync(message2 as T1, message1 as T2);
            }
        }
        public object GetInnerObject()
        {
            if (_handler != null)
            {
                return _handler;
            }
            return ObjectContainer.Resolve(_handlerType);
        }
    }
    public class MessageHandlerProxy3<T1, T2, T3> : IMessageHandlerProxy3
        where T1 : class, IMessage
        where T2 : class, IMessage
        where T3 : class, IMessage
    {
        private IMessageHandler<T1, T2, T3> _handler;
        private readonly Type _handlerType;

        public MessageHandlerProxy3(IMessageHandler<T1, T2, T3> handler, Type handlerType)
        {
            _handler = handler;
            _handlerType = handlerType;
        }

        public Task<AsyncTaskResult> HandleAsync(IMessage message1, IMessage message2, IMessage message3)
        {
            T1 t1 = null;
            T2 t2 = null;
            T3 t3 = null;

            if (message1 is T1)
            {
                t1 = message1 as T1;
            }
            else if (message2 is T1)
            {
                t1 = message2 as T1;
            }
            else if (message3 is T1)
            {
                t1 = message3 as T1;
            }

            if (message2 is T2)
            {
                t2 = message2 as T2;
            }
            else if (message3 is T2)
            {
                t2 = message3 as T2;
            }
            else if (message1 is T2)
            {
                t2 = message1 as T2;
            }

            if (message3 is T3)
            {
                t3 = message3 as T3;
            }
            else if (message2 is T3)
            {
                t3 = message2 as T3;
            }
            else if (message1 is T3)
            {
                t3 = message1 as T3;
            }

            var handler = GetInnerObject() as IMessageHandler<T1, T2, T3>;
            return handler.HandleAsync(t1, t2, t3);
        }
        public object GetInnerObject()
        {
            if (_handler != null)
            {
                return _handler;
            }
            return ObjectContainer.Resolve(_handlerType);
        }
    }
}
