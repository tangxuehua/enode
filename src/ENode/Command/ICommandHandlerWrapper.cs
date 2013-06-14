using System;

namespace ENode.Commanding
{
    /// <summary>Represents a command handler wrapper interface.
    /// </summary>
    public interface ICommandHandlerWrapper
    {
        object GetInnerCommandHandler();
    }
}
