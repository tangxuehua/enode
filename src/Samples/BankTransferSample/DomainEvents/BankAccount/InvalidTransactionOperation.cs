using System;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>在转账交易已完成后进行了无效的操作
    /// </summary>
    [Serializable]
    public class InvalidTransactionOperation : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }
        public TransactionOperationType OperationType { get; private set; }

        public InvalidTransactionOperation(string accountId, Guid transactionId, TransactionOperationType operationType) : base(accountId)
        {
            TransactionId = transactionId;
            OperationType = operationType;
        }
    }
    /// <summary>转账交易操作类型枚举
    /// </summary>
    public enum TransactionOperationType
    {
        PrepareDebit,
        PrepareCredit,
        CommitDebit,
        CommitCredit,
        AbortDebit,
        AbortCredit
    }
}
