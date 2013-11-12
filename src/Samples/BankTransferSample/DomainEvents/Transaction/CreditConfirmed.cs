using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents.Transaction
{
    /// <summary>交易转入已确认
    /// </summary>
    [Serializable]
    public class CreditConfirmed : DomainEvent<Guid>, ISourcingEvent
    {
        public TransactionInfo TransactionInfo { get; private set; }
        public DateTime ConfirmedTime { get; private set; }

        public CreditConfirmed(Guid transactionId, TransactionInfo transactionInfo, DateTime confirmedTime) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
            ConfirmedTime = confirmedTime;
        }
    }
}
