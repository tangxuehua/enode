using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events
{
    [Serializable]
    public class TransferedOut : AbstractTransferEvent
    {
        public TransferedOut(Guid processId, TransferInfo transferInfo, string description)
            : base(processId, transferInfo, description)
        {
        }
    }
}
