using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents.Transaction
{
    /// <summary>交易已完成
    /// </summary>
    [Serializable]
    public class TransactionCompleted : DomainEvent<Guid>, ISourcingEvent
    {
        public TransactionInfo TransactionInfo { get; private set; }
        public DateTime CompletedTime { get; private set; }

        public TransactionCompleted(Guid transactionId, TransactionInfo transactionInfo, DateTime completedTime) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
            CompletedTime = completedTime;
        }
    }
}
