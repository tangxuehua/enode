using System;
using BankTransferSagaSample.Domain;
using BankTransferSagaSample.Events;
using ENode.Commanding;

namespace BankTransferSagaSample.Commands
{
    [Serializable]
    public class OpenAccount : Command
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string Customer { get; set; }
    }
    [Serializable]
    public class Deposit : Command
    {
        public Guid AccountId { get; set; }
        public long Amount { get; set; }
    }

    [Serializable]
    public abstract class AbstractTransferCommand : Command
    {
        public Guid ProcessId { get; set; }
        public TransferInfo TransferInfo { get; set; }
    }

    [Serializable]
    public class Transfer : AbstractTransferCommand
    {
        public Transfer()
        {
            ProcessId = Guid.NewGuid();
        }
    }
    [Serializable]
    public class TransferOut : AbstractTransferCommand
    {
    }
    [Serializable]
    public class TransferIn : AbstractTransferCommand
    {
    }
    [Serializable]
    public class HandleTransferedOut : AbstractTransferCommand
    {
    }
    [Serializable]
    public class HandleTransferedIn : AbstractTransferCommand
    {
    }
    [Serializable]
    public class HandleTransferOutFail : AbstractTransferCommand
    {
        public string ErrorMessage { get; set; }
    }
    [Serializable]
    public class HandleTransferInFail : AbstractTransferCommand
    {
        public string ErrorMessage { get; set; }
    }
    [Serializable]
    public class RollbackTransferOut : AbstractTransferCommand
    {
    }
    [Serializable]
    public class CompleteTransfer : AbstractTransferCommand
    {
    }
}
