using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events
{
    /// <summary>转账流程已正常完成
    /// </summary>
    [Serializable]
    public class TransferProcessCompleted : AbstractTransferEvent
    {
        public TransferProcessCompleted(Guid processId, TransferInfo transferInfo)
            : base(processId, transferInfo)
        {
        }
    }
}
