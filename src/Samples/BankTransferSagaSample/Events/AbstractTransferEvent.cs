using System;
using BankTransferSagaSample.Domain;
using ENode.Eventing;

namespace BankTransferSagaSample.Events
{
    /// <summary>与转账流程相关的抽象事件，包含转账流程ID和基本的转账信息
    /// </summary>
    [Serializable]
    public abstract class AbstractTransferEvent : Event
    {
        public Guid ProcessId { get; private set; }
        public TransferInfo TransferInfo { get; private set; }
        public string Description { get; private set; }

        public AbstractTransferEvent(Guid processId, TransferInfo transferInfo)
            : this(processId, transferInfo, null)
        {
        }
        public AbstractTransferEvent(Guid processId, TransferInfo transferInfo, string description)
        {
            ProcessId = processId;
            TransferInfo = transferInfo;
            Description = description;
        }
    }
}
