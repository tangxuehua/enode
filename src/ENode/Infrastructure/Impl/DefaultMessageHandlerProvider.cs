using System;
using System.Linq;
using System.Threading.Tasks;
using ECommon.IO;
using ENode.Eventing;

namespace ENode.Infrastructure.Impl
{
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
