using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events {
    /// <summary>转入的请求已发起
    /// </summary>
    [Serializable]
    public class TransferInRequested : AbstractTransferEvent {
        public TransferInRequested(Guid processId, TransferInfo transferInfo)
            : base(processId, transferInfo) {
        }
    }
}
