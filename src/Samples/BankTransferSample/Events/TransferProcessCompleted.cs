using System;
using BankTransferSample.Domain;

namespace BankTransferSample.Events
{
    /// <summary>转账流程已正常完成
    /// </summary>
    [Serializable]
    public class TransferProcessCompleted : AbstractTransferEvent
    {
        /// <summary>Represents the process result.
        /// </summary>
        public TransactionResult ProcessResult { get; protected set; }

        public TransferProcessCompleted(Guid processId, TransferInfo transferInfo, TransactionResult processResult)
            : base(processId, processId, transferInfo)
        {
            ProcessResult = processResult;
        }
    }
}
