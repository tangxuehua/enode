using System;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>交易预转入信息不存在
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class CreditPreparationNotExist  : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }

        public CreditPreparationNotExist(string accountId, Guid transactionId) : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
