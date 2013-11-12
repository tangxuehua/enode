using System;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>重复的预转入操作
    /// </summary>
    [Serializable]
    public class DuplicatedCreditPreparation : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }

        public DuplicatedCreditPreparation(string accountId, Guid transactionId) : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
