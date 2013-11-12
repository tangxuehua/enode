using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents.Transaction
{
    /// <summary>交易已提交
    /// </summary>
    [Serializable]
    public class TransactionCommitted : DomainEvent<Guid>, ISourcingEvent
    {
        public TransactionInfo TransactionInfo { get; private set; }
        public DateTime CommittedTime { get; private set; }

        public TransactionCommitted(Guid transactionId, TransactionInfo transactionInfo, DateTime committedTime) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
            CommittedTime = committedTime;
        }
    }
}
