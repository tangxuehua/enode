using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events {
    /// <summary>转出已回滚
    /// </summary>
    [Serializable]
    public class TransferOutRolledback : AbstractTransferEvent {
        public TransferOutRolledback(Guid processId, TransferInfo transferInfo, string description)
            : base(processId, transferInfo, description) {
        }
    }
}
