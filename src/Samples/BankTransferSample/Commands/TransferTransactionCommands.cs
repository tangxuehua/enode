using System;
using BankTransferSample.Domain;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>发起一笔转账交易
    /// </summary>
    [Serializable]
    public class StartTransferTransactionCommand : ProcessCommand<string>, ICreatingAggregateCommand
    {
        public TransferTransactionInfo TransactionInfo { get; private set; }

        public StartTransferTransactionCommand(TransferTransactionInfo transactionInfo)
        {
            TransactionInfo = transactionInfo;
        }
    }
    /// <summary>确认预转出
    /// </summary>
    [Serializable]
    public class ConfirmTransferOutPreparationCommand : ProcessCommand<string>
    {
        public ConfirmTransferOutPreparationCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>确认预转入
    /// </summary>
    [Serializable]
    public class ConfirmTransferInPreparationCommand : ProcessCommand<string>
    {
        public ConfirmTransferInPreparationCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>确认转出
    /// </summary>
    [Serializable]
    public class ConfirmTransferOutCommand : ProcessCommand<string>
    {
        public ConfirmTransferOutCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>确认转入
    /// </summary>
    [Serializable]
    public class ConfirmTransferInCommand : ProcessCommand<string>
    {
        public ConfirmTransferInCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>取消转账交易
    /// </summary>
    [Serializable]
    public class CancelTransferTransactionCommand : ProcessCommand<string>
    {
        public CancelTransferTransactionCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
}
