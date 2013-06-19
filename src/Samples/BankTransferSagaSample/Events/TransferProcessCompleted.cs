using System;
using ENode.Eventing;

namespace BankTransferSagaSample.Events
{
    [Serializable]
    public class TransferProcessCompleted : Event
    {
        public Guid ProcessId { get; private set; }

        public TransferProcessCompleted(Guid processId)
        {
            ProcessId = processId;
        }
    }
}
