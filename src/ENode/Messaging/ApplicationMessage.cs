using System;

namespace ENode.Messaging
{
    /// <summary>Represents an abstract application message.
    /// </summary>
    [Serializable]
    public abstract class ApplicationMessage : Message, IApplicationMessage
    {
    }
}
