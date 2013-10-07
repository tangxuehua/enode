using System;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>开户
    /// </summary>
    [Serializable]
    public class OpenAccount : Command
    {
        public string AccountId { get; set; }
        public string Owner { get; set; }
    }
    /// <summary>存款
    /// </summary>
    [Serializable]
    public class Deposit : Command
    {
        public string AccountId { get; set; }
        public long Amount { get; set; }
    }
}
