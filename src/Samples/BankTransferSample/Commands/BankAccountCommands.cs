using System;
using BankTransferSample.Domain;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>开户（创建一个账户）
    /// </summary>
    [Serializable]
    public class CreateAccountCommand : AggregateCommand<string>, ICreatingAggregateCommand
    {
        public string Owner { get; set; }

        public CreateAccountCommand() { }
        public CreateAccountCommand(string accountId, string owner) : base(accountId)
        {
            Owner = owner;
        }
    }
    /// <summary>验证账户是否合法
    /// </summary>
    [Serializable]
    public class ValidateAccountCommand : Command
    {
        public string AccountId { get; set; }
        public string TransactionId { get; set; }

        public ValidateAccountCommand() { }
        public ValidateAccountCommand(string accountId, string transactionId)
        {
            AccountId = accountId;
            TransactionId = transactionId;
        }

        public override object GetKey()
        {
            return AccountId;
        }
    }
    /// <summary>向账户添加一笔预操作
    /// </summary>
    [Serializable]
    public class AddTransactionPreparationCommand : AggregateCommand<string>
    {
        public string TransactionId { get; set; }
        public TransactionType TransactionType { get; set; }
        public PreparationType PreparationType { get; set; }
        public double Amount { get; set; }

        public AddTransactionPreparationCommand() { }
        public AddTransactionPreparationCommand(string accountId, string transactionId, TransactionType transactionType, PreparationType preparationType, double amount)
            : base(accountId)
        {
            TransactionId = transactionId;
            TransactionType = transactionType;
            PreparationType = preparationType;
            Amount = amount;
        }
    }
    /// <summary>提交预操作
    /// </summary>
    [Serializable]
    public class CommitTransactionPreparationCommand : AggregateCommand<string>
    {
        public string TransactionId { get; set; }

        public CommitTransactionPreparationCommand() { }
        public CommitTransactionPreparationCommand(string accountId, string transactionId)
            : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
