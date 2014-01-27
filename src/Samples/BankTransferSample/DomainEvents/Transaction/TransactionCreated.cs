using System;
using BankTransferSample.Domain;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.Transaction
{
    /// <summary>交易已创建
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class TransactionCreated : DomainEvent<Guid>, ISourcingEvent
    {
        public TransactionInfo TransactionInfo { get; private set; }

        public TransactionCreated(TransactionInfo transactionInfo)
            : base(transactionInfo.TransactionId)
        {
            TransactionInfo = transactionInfo;
        }
    }
}
