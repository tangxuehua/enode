using System;
using ENode.Infrastructure.Impl;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandHandlerProvider : AbstractHandlerProvider<ICommandHandlerProxy>, ICommandHandlerProvider
    {
        protected override Type GetGenericHandlerType()
        {
            return typeof(ICommandHandler<>);
        }
        protected override Type GetHandlerProxyImplementationType()
        {
            return typeof(CommandHandlerProxy<>);
        }
    }
}
