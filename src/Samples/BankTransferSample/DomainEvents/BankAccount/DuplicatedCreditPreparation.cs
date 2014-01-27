using System;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>重复的预转入操作
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class DuplicatedCreditPreparation : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }

        public DuplicatedCreditPreparation(string accountId, Guid transactionId) : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
