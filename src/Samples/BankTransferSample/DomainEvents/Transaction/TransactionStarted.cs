using System;
using BankTransferSample.Domain;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.Transaction
{
    /// <summary>交易已开始
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class TransactionStarted : DomainEvent<Guid>, ISourcingEvent
    {
        public TransactionInfo TransactionInfo { get; private set; }
        public DateTime StartedTime { get; private set; }

        public TransactionStarted(Guid transactionId, TransactionInfo transactionInfo, DateTime startedTime) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
            StartedTime = startedTime;
        }
    }
}
