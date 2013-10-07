using System;
using BankTransferSample.Domain;

namespace BankTransferSample.Events
{
    /// <summary>钱已转出
    /// </summary>
    [Serializable]
    public class TransferedOut : AbstractTransferEvent
    {
        public TransferedOut(Guid processId, TransferInfo transferInfo, string description)
            : base(transferInfo.SourceAccountId, processId, transferInfo, description)
        {
        }
    }
}
