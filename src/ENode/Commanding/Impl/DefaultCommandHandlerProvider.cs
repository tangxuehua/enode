using System;
using System.Linq;
using ENode.Infrastructure.Impl;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandHandlerProvider : AbstractHandlerProvider<Type, ICommandHandlerProxy, Type>, ICommandHandlerProvider
    {
        protected override Type GetGenericHandlerType()
        {
            return typeof(ICommandHandler<>);
        }
        protected override Type GetKey(Type handlerInterfaceType)
        {
            return handlerInterfaceType.GetGenericArguments().Single();
        }
        protected override Type GetHandlerProxyImplementationType(Type handlerInterfaceType)
        {
            return typeof(CommandHandlerProxy<>).MakeGenericType(handlerInterfaceType.GetGenericArguments().Single());
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
