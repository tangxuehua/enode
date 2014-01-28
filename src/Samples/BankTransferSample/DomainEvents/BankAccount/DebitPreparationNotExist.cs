using System;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>交易预转出信息不存在
    /// </summary>
    [Serializable]
    public class DebitPreparationNotExist  : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }

        public DebitPreparationNotExist(string accountId, Guid transactionId) : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
