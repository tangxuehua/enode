using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events
{
    /// <summary>回滚转出的请求已发起
    /// </summary>
    [Serializable]
    public class RollbackTransferOutRequested : AbstractTransferEvent
    {
        public string ErrorMessage { get; private set; }

        public RollbackTransferOutRequested(Guid processId, TransferInfo transferInfo, string errorMessage)
            : base(processId, transferInfo)
        {
            ErrorMessage = errorMessage;
        }
    }
}
