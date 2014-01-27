using System;
using BankTransferSample.Domain;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.Transaction
{
    /// <summary>交易预转入已确认
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class CreditPreparationConfirmed : DomainEvent<Guid>, ISourcingEvent
    {
        public TransactionInfo TransactionInfo { get; private set; }
        public DateTime ConfirmedTime { get; private set; }

        public CreditPreparationConfirmed(Guid transactionId, TransactionInfo transactionInfo, DateTime confirmedTime) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
            ConfirmedTime = confirmedTime;
        }
    }
}
