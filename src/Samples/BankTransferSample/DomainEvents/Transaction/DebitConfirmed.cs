using System;
using BankTransferSample.Domain;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.Transaction
{
    /// <summary>交易转出已确认
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class DebitConfirmed : DomainEvent<Guid>, ISourcingEvent
    {
        public TransactionInfo TransactionInfo { get; private set; }
        public DateTime ConfirmedTime { get; private set; }

        public DebitConfirmed(Guid transactionId, TransactionInfo transactionInfo, DateTime confirmedTime) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
            ConfirmedTime = confirmedTime;
        }
    }
}
