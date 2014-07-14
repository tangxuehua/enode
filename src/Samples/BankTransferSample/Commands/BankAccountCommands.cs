using System;
using BankTransferSample.Domain;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>开户（创建一个账户）
    /// </summary>
    [Serializable]
    public class CreateAccountCommand : Command<string>, ICreatingAggregateCommand
    {
        public string Owner { get; set; }

        public CreateAccountCommand(string accountId, string owner) : base(accountId)
        {
            Owner = owner;
        }
    }
    /// <summary>向账户添加一笔预操作
    /// </summary>
    [Serializable]
    public class AddTransactionPreparationCommand : ProcessCommand<string>
    {
        public string TransactionId { get; set; }
        public TransactionType TransactionType { get; set; }
        public PreparationType PreparationType { get; set; }
        public double Amount { get; set; }

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
    public class CommitTransactionPreparationCommand : ProcessCommand<string>
    {
        public string TransactionId { get; set; }

        public CommitTransactionPreparationCommand(string accountId, string transactionId)
            : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
    /// <summary>取消预操作
    /// </summary>
    [Serializable]
    public class CancelTransactionPreparationCommand : ProcessCommand<string>
    {
        public string TransactionId { get; set; }

        public CancelTransactionPreparationCommand(string accountId, string transactionId)
            : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
