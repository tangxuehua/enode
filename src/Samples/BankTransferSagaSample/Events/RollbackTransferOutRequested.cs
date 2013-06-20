using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events
{
    /// <summary>回滚转出的请求已发起
    /// </summary>
    [Serializable]
    public class RollbackTransferOutRequested : AbstractTransferEvent
    {
        public RollbackTransferOutRequested(Guid processId, TransferInfo transferInfo)
            : base(processId, transferInfo)
        {
        }
    }
}
