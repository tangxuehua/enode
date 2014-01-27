using System;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>交易预转入成功
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class CreditPrepared : SourcingEvent<string>
    {
        public Guid TransactionId { get; private set; }
        public double Amount { get; private set; }

        public CreditPrepared(string accountId, Guid transactionId, double amount) : base(accountId)
        {
            TransactionId = transactionId;
            Amount = amount;
        }
    }
}
