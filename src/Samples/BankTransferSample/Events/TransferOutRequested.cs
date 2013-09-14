using System;
using BankTransferSample.Domain;

namespace BankTransferSample.Events
{
    /// <summary>转出的请求已发起
    /// </summary>
    [Serializable]
    public class TransferOutRequested : AbstractTransferEvent
    {
        public TransferOutRequested(Guid processId, TransferInfo transferInfo)
            : base(processId, transferInfo)
        {
        }
    }
}
