using System;
using BankTransferSagaSample.Domain;
using ENode.Eventing;

namespace BankTransferSagaSample.Events
{
    [Serializable]
    public class TransferProcessStarted : AbstractTransferEvent
    {
        public TransferProcessStarted(Guid processId, TransferInfo transferInfo, string description)
            : base(processId, transferInfo, description)
        {
        }
    }
}
