using System;
using BankTransferSample.Domain;

namespace BankTransferSample.Events
{
    /// <summary>钱已转入
    /// </summary>
    [Serializable]
    public class TransferedIn : AbstractTransferEvent
    {
        public TransferedIn(Guid processId, TransferInfo transferInfo, string description)
            : base(transferInfo.TargetAccountId, processId, transferInfo, description)
        {
        }
    }
}
