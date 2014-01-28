using System;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>重复的预转出操作
    /// </summary>
    [Serializable]
    public class DuplicatedDebitPreparation : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }

        public DuplicatedDebitPreparation(string accountId, Guid transactionId) : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
