using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events
{
    /// <summary>回滚转出的请求已发起
    /// </summary>
    [Serializable]
    public class RollbackTransferOutRequested : AbstractTransferEvent
    {
        public Exception ProcessException { get; private set; }

        public RollbackTransferOutRequested(Guid processId, TransferInfo transferInfo, Exception processException)
            : base(processId, transferInfo)
        {
            ProcessException = processException;
        }
    }
}
