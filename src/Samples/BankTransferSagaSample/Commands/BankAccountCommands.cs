using System;
using BankTransferSagaSample.Domain;
using BankTransferSagaSample.Events;
using ENode.Commanding;

namespace BankTransferSagaSample.Commands
{
    /// <summary>开户
    /// </summary>
    [Serializable]
    public class OpenAccount : Command
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string Owner { get; set; }
    }
    /// <summary>存款
    /// </summary>
    [Serializable]
    public class Deposit : Command
    {
        public Guid AccountId { get; set; }
        public long Amount { get; set; }
    }
}
