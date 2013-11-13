using System;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>开户
    /// </summary>
    [Serializable]
    public class CreateAccount : Command
    {
        public string AccountId { get; set; }
        public string Owner { get; set; }

        public CreateAccount(string accountId, string owner)
        {
            AccountId = accountId;
            Owner = owner;
        }
    }
    /// <summary>存款
    /// </summary>
    [Serializable]
    public class Deposit : Command
    {
        public string AccountId { get; set; }
        public double Amount { get; set; }

        public Deposit(string accountId, double amount)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }
    /// <summary>取款
    /// </summary>
    [Serializable]
    public class Withdraw : Command
    {
        public string AccountId { get; set; }
        public double Amount { get; set; }

        public Withdraw(string accountId, double amount)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }
    /// <summary>预转出
    /// </summary>
    [Serializable]
    public class PrepareDebit : Command
    {
        public string AccountId { get; set; }
        public Guid TransactionId { get; set; }
        public double Amount { get; set; }

        public PrepareDebit(string accountId, Guid transactionId, double amount)
        {
            AccountId = accountId;
            TransactionId = transactionId;
            Amount = amount;
        }
    }
    /// <summary>预转入
    /// </summary>
    [Serializable]
    public class PrepareCredit : Command
    {
        public string AccountId { get; set; }
        public Guid TransactionId { get; set; }
        public double Amount { get; set; }

        public PrepareCredit(string accountId, Guid transactionId, double amount)
        {
            AccountId = accountId;
            TransactionId = transactionId;
            Amount = amount;
        }
    }
    /// <summary>提交转出
    /// </summary>
    [Serializable]
    public class CommitDebit : Command
    {
        public string AccountId { get; set; }
        public Guid TransactionId { get; set; }

        public CommitDebit(string accountId, Guid transactionId)
        {
            AccountId = accountId;
            TransactionId = transactionId;
        }
    }
    /// <summary>提交转入
    /// </summary>
    [Serializable]
    public class CommitCredit : Command
    {
        public string AccountId { get; set; }
        public Guid TransactionId { get; set; }

        public CommitCredit(string accountId, Guid transactionId)
        {
            AccountId = accountId;
            TransactionId = transactionId;
        }
    }
    /// <summary>终止转出
    /// </summary>
    [Serializable]
    public class AbortDebit : Command
    {
        public string AccountId { get; set; }
        public Guid TransactionId { get; set; }

        public AbortDebit(string accountId, Guid transactionId)
        {
            AccountId = accountId;
            TransactionId = transactionId;
        }
    }
    /// <summary>终止转入
    /// </summary>
    [Serializable]
    public class AbortCredit : Command
    {
        public string AccountId { get; set; }
        public Guid TransactionId { get; set; }

        public AbortCredit(string accountId, Guid transactionId)
        {
            AccountId = accountId;
            TransactionId = transactionId;
        }
    }
}
