using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents.Transaction
{
    /// <summary>交易已创建
    /// </summary>
    [Serializable]
    public class TransactionCreated : DomainEvent<Guid>, ISourcingEvent
    {
        public TransactionInfo TransactionInfo { get; private set; }

        public TransactionCreated(Guid transactionId, TransactionInfo transactionInfo) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
        }
    }
}
