using System;

namespace ENode.Eventing.Impl
{
    /// <summary>Event process status enum.
    /// </summary>
    public enum EventProcessStatus
    {
        Success = 1,
        SynchronizerConcurrentException,
        SynchronizerFailed,
        ConcurrentException,
        PublishFailed,
        Failed
    }
}
