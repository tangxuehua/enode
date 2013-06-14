using System;
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
    public class Transfer : Command
    {
        public Guid ProcessId { get; set; }
        public Guid SourceAccountId { get; set; }
        public Guid TargetAccountId { get; set; }
        public long Amount { get; set; }

        public Transfer()
        {
            ProcessId = Guid.NewGuid();
        }
    }
    [Serializable]
    public class TransferOut : Command
    {
        public Guid ProcessId { get; set; }
        public Guid SourceAccountId { get; set; }
        public Guid TargetAccountId { get; set; }
        public double Amount { get; set; }
    }
    [Serializable]
    public class TransferIn : Command
    {
        public Guid ProcessId { get; set; }
        public Guid SourceAccountId { get; set; }
        public Guid TargetAccountId { get; set; }
        public double Amount { get; set; }
    }
    [Serializable]
    public class HandleTransferedOut : Command
    {
        public TransferedOut Event { get; private set; }

        public HandleTransferedOut(TransferedOut evnt)
        {
            Event = evnt;
        }
    }
    [Serializable]
    public class HandleTransferedIn : Command
    {
        public TransferedIn Event { get; private set; }

        public HandleTransferedIn(TransferedIn evnt)
        {
            Event = evnt;
        }
    }
}
