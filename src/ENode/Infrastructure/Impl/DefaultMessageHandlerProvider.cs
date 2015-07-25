using System;
using System.Linq;
using System.Threading.Tasks;
using ECommon.IO;
using ENode.Eventing;

namespace ENode.Infrastructure.Impl
{

    //class Event01 : IDomainEvent { }
    //class Event02 : IDomainEvent { }

    //class CommandHandler : IMessageHandler<IDomainEvent>, IMessageHandler<Event01>
    //{
    //    [Priority(1)]
    //    public Task<AsyncTaskResult> HandleAsync(IDomainEvent message) { return null; }
    //    [Priority(2)]
    //    public Task<AsyncTaskResult> HandleAsync(Event01 message) { return null; }
    //}
    //class CommandHandler2 : IMessageHandler<IDomainEvent>
    //{
    //    [Priority(3)]
    //    public Task<AsyncTaskResult> HandleAsync(IDomainEvent message) { return null; }
    //}
    //class Event01Handler01 : IMessageHandler<Event01>
    //{
    //    [Priority(1)]
    //    public Task<AsyncTaskResult> HandleAsync(Event01 message) { return null; }
    //}
    //class Event01Handler02 : IMessageHandler<Event01>
    //{
    //    [Priority(2)]
    //    public Task<AsyncTaskResult> HandleAsync(Event01 message) { return null; }
    //}
    //class Event02Handler01 : IMessageHandler<Event02>
    //{
    //    [Priority(1)]
    //    public Task<AsyncTaskResult> HandleAsync(Event02 message) { return null; }
    //}
    //class Event02Handler02 : IMessageHandler<Event02>
    //{
    //    [Priority(2)]
    //    public Task<AsyncTaskResult> HandleAsync(Event02 message) { return null; }
    //}

    public class DefaultMessageHandlerProvider : AbstractHandlerProvider<Type, IMessageHandlerProxy1, Type>, IMessageHandlerProvider
    {
        protected override Type GetGenericHandlerType()
        {
            return typeof(IMessageHandler<>);
        }
        protected override Type GetKey(Type handlerInterfaceType)
        {
            return handlerInterfaceType.GetGenericArguments().Single();
        }
        protected override Type GetHandlerProxyImplementationType(Type handlerInterfaceType)
        {
            return typeof(MessageHandlerProxy1<>).MakeGenericType(handlerInterfaceType.GetGenericArguments().Single());
        }
        protected override bool IsHandlerSourceMatchKey(Type handlerSource, Type key)
        {
            return key.IsAssignableFrom(handlerSource);
        }
        protected override bool IsHandleMethodMatchKey(Type[] argumentTypes, Type key)
        {
            return argumentTypes.Count() == 1 && argumentTypes[0] == key;
        }
    }
}
