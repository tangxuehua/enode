using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>转账流程已开始
    /// </summary>
    [Serializable]
    public class TransactionStarted : SourcableDomainEvent<Guid>
    {
        public TransactionInfo TransactionInfo { get; private set; }

        public TransactionStarted(Guid transactionId, TransactionInfo transactionInfo) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
        }
    }
}
