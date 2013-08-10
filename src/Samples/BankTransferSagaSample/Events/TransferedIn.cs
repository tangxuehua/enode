using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events {
    /// <summary>钱已转入
    /// </summary>
    [Serializable]
    public class TransferedIn : AbstractTransferEvent {
        public TransferedIn(Guid processId, TransferInfo transferInfo, string description)
            : base(processId, transferInfo, description) {
        }
    }
}
