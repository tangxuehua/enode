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
    /// <summary>完成转出
    /// </summary>
    [Serializable]
    public class CompleteDebit : Command
    {
        public string AccountId { get; set; }
        public Guid TransactionId { get; set; }

        public CompleteDebit(string accountId, Guid transactionId)
        {
            AccountId = accountId;
            TransactionId = transactionId;
        }
    }
    /// <summary>完成转入
    /// </summary>
    [Serializable]
    public class CompleteCredit : Command
    {
        public string AccountId { get; set; }
        public Guid TransactionId { get; set; }

        public CompleteCredit(string accountId, Guid transactionId)
        {
            AccountId = accountId;
            TransactionId = transactionId;
        }
    }
}
