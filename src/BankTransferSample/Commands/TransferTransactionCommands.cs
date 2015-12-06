using System;
using BankTransferSample.Domain;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSample.Commands
{
    /// <summary>发起一笔转账交易
    /// </summary>
    public class StartTransferTransactionCommand : Command
    {
        /// <summary>转账交易信息
        /// </summary>
        public TransferTransactionInfo TransactionInfo { get; set; }

        public StartTransferTransactionCommand() { }
        public StartTransferTransactionCommand(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId)
        {
            TransactionInfo = transactionInfo;
        }
    }
    /// <summary>确认账户验证已通过
    /// </summary>
    public class ConfirmAccountValidatePassedCommand : Command
    {
        /// <summary>账户ID
        /// </summary>
        public string AccountId { get; set; }

        public ConfirmAccountValidatePassedCommand() { }
        public ConfirmAccountValidatePassedCommand(string transactionId, string accountId)
            : base(transactionId)
        {
            AccountId = accountId;
        }
    }
    /// <summary>确认预转出
    /// </summary>
    public class ConfirmTransferOutPreparationCommand : Command
    {
        public ConfirmTransferOutPreparationCommand() { }
        public ConfirmTransferOutPreparationCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>确认预转入
    /// </summary>
    public class ConfirmTransferInPreparationCommand : Command
    {
        public ConfirmTransferInPreparationCommand() { }
        public ConfirmTransferInPreparationCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>确认转出
    /// </summary>
    public class ConfirmTransferOutCommand : Command
    {
        public ConfirmTransferOutCommand() { }
        public ConfirmTransferOutCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>确认转入
    /// </summary>
    public class ConfirmTransferInCommand : Command
    {
        public ConfirmTransferInCommand() { }
        public ConfirmTransferInCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>取消转账交易
    /// </summary>
    public class CancelTransferTransactionCommand : Command
    {
        public CancelTransferTransactionCommand() { }
        public CancelTransferTransactionCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
}
