using System;
using BankTransferSagaSample.Domain;
using BankTransferSagaSample.Events;
using ENode.Commanding;

namespace BankTransferSagaSample.Commands
{
    [Serializable]
    public abstract class AbstractTransferCommand : Command
    {
        public Guid ProcessId { get; set; }
        public TransferInfo TransferInfo { get; set; }
    }
    [Serializable]
    public class StartTransfer : AbstractTransferCommand
    {
        public StartTransfer()
        {
            ProcessId = Guid.NewGuid();
        }
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
    public class HandleFailedTransferOut : AbstractTransferCommand
    {
    }
    [Serializable]
    public class HandleFailedTransferIn : AbstractTransferCommand
    {
    }
    [Serializable]
    public class HandleTransferOutRolledback : AbstractTransferCommand
    {
    }
}
