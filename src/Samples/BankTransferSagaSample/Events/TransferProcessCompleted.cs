using System;
using BankTransferSagaSample.Domain;
using ENode.Domain;

namespace BankTransferSagaSample.Events
{
    /// <summary>转账流程已正常完成
    /// </summary>
    [Serializable]
    public class TransferProcessCompleted : AbstractTransferEvent
    {
        /// <summary>Represents the process result.
        /// </summary>
        public ProcessResult ProcessResult { get; protected set; }

        public TransferProcessCompleted(Guid processId, TransferInfo transferInfo, ProcessResult processResult)
            : base(processId, transferInfo)
        {
            ProcessResult = processResult;
        }
    }
}
