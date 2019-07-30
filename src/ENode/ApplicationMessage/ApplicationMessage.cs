using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents an abstract application message.
    /// </summary>
    [Serializable]
    public abstract class ApplicationMessage : Message, IApplicationMessage
    {
    }
}
