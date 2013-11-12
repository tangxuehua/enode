using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易预转出信息已存在
    /// </summary>
    [Serializable]
    public class DebitPreparationExist  : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }

        public DebitPreparationExist(string accountId, Guid transactionId) : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
