using System;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IMessageHandlerProvider<ICommandHandler>.
    /// </summary>
    public class DefaultCommandHandlerProvider : BaseHandlerProvider<ICommandHandler>, IMessageHandlerProvider<ICommandHandler>, IAssemblyInitializer
    {
        protected override Type GetMessageHandlerGenericInterfaceType()
        {
            return typeof(ICommandHandler<>);
        }
        protected override Type GetMessageHandlerWrapperType()
        {
            return typeof(CommandHandlerWrapper<>);
        }
    }
}
