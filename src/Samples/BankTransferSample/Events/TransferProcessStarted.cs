using System;
using BankTransferSample.Domain;

namespace BankTransferSample.Events
{
    /// <summary>转账流程已开始
    /// </summary>
    [Serializable]
    public class TransferProcessStarted : AbstractTransferEvent
    {
        public TransferProcessStarted(Guid processId, TransferInfo transferInfo, string description)
            : base(processId, transferInfo, description)
        {
        }
    }
}
