using System;
using BankTransferSample.Domain;

namespace BankTransferSample.Events
{
    /// <summary>转出已回滚
    /// </summary>
    [Serializable]
    public class TransferOutRolledback : AbstractTransferEvent
    {
        public TransferOutRolledback(Guid processId, TransferInfo transferInfo, string description)
            : base(transferInfo.SourceAccountId, processId, transferInfo, description)
        {
        }
    }
}
