using System;
using System.Linq;
using ENode.Infrastructure.Impl;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandAsyncHandlerProvider : AbstractHandlerProvider<Type, ICommandAsyncHandlerProxy, Type>, ICommandAsyncHandlerProvider
    {
        protected override Type GetGenericHandlerType()
        {
            return typeof(ICommandAsyncHandler<>);
        }
        protected override Type GetKey(Type handlerInterfaceType)
        {
            return handlerInterfaceType.GetGenericArguments().Single();
        }
        protected override Type GetHandlerProxyImplementationType(Type handlerInterfaceType)
        {
            return typeof(CommandAsyncHandlerProxy<>).MakeGenericType(handlerInterfaceType.GetGenericArguments().Single());
        }
        protected override bool IsHandlerSourceMatchKey(Type handlerSource, Type key)
        {
            return key == handlerSource;
        }
        protected override bool IsHandleMethodMatchKey(Type[] argumentTypes, Type key)
        {
            return argumentTypes.Count() == 1 && argumentTypes[0] == key;
        }
    }
}
