using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events
{
    /// <summary>转账流程已异常终止
    /// </summary>
    [Serializable]
    public class TransferProcessAborted : AbstractTransferEvent
    {
        public TransferProcessAborted(Guid processId, TransferInfo transferInfo)
            : base(processId, transferInfo)
        {
        }
    }
}
