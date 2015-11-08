using System;
using BankTransferSample.Domain;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSample.Commands
{
    /// <summary>发起一笔转账交易
    /// </summary>
    [Serializable]
    [Code(107)]
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
    [Serializable]
    [Code(108)]
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
    [Serializable]
    [Code(109)]
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
    [Serializable]
    [Code(110)]
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
    [Serializable]
    [Code(111)]
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
    [Serializable]
    [Code(112)]
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
    [Serializable]
    [Code(113)]
    public class CancelTransferTransactionCommand : Command
    {
        public CancelTransferTransactionCommand() { }
        public CancelTransferTransactionCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
}
