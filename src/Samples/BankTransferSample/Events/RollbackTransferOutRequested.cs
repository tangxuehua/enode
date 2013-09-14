using System;
using BankTransferSagaSample.Domain;
using ENode.Infrastructure;

namespace BankTransferSagaSample.Events
{
    /// <summary>回滚转出的请求已发起
    /// </summary>
    [Serializable]
    public class RollbackTransferOutRequested : AbstractTransferEvent
    {
        public ErrorInfo ErrorInfo { get; private set; }

        public RollbackTransferOutRequested(Guid processId, TransferInfo transferInfo, ErrorInfo errorInfo)
            : base(processId, transferInfo)
        {
            ErrorInfo = errorInfo;
        }
    }
}
