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
        public string Owner { get; set; }
    }
    [Serializable]
    public class Deposit : Command
    {
        public Guid AccountId { get; set; }
        public long Amount { get; set; }
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
    public class RollbackTransferOut : AbstractTransferCommand
    {
    }
}
