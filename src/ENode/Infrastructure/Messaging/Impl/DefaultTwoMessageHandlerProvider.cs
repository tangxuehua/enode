using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.Infrastructure.Impl
{
    public class DefaultTwoMessageHandlerProvider : AbstractHandlerProvider<ManyType, IMessageHandlerProxy2, IEnumerable<Type>>, ITwoMessageHandlerProvider
    {
        protected override Type GetGenericHandlerType()
        {
            return typeof(IMessageHandler<,>);
        }
        protected override ManyType GetKey(Type handlerInterfaceType)
        {
            return new ManyType(handlerInterfaceType.GetGenericArguments());
        }
        protected override Type GetHandlerProxyImplementationType(Type handlerInterfaceType)
        {
            return typeof(MessageHandlerProxy2<,>).MakeGenericType(handlerInterfaceType.GetGenericArguments());
        }
        protected override bool IsHandlerSourceMatchKey(IEnumerable<Type> handlerSource, ManyType key)
        {
            foreach (var type in key.GetTypes())
            {
                if (!handlerSource.Any(x => x == type))
                {
                    return false;
                }
            }
            return true;
        }
        protected override bool IsHandleMethodMatchKey(Type[] argumentTypes, ManyType key)
        {
            return argumentTypes.Length == key.GetTypes().Count() && key.GetTypes().Any(x => argumentTypes.Any(y => y == x));
        }
    }
}
