using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events
{
    /// <summary>钱已转出
    /// </summary>
    [Serializable]
    public class TransferedOut : AbstractTransferEvent
    {
        public TransferedOut(Guid processId, TransferInfo transferInfo, string description)
            : base(processId, transferInfo, description)
        {
        }
    }
}
