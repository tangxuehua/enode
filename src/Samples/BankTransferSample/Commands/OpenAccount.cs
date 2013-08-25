using System;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    [Serializable]
    public class OpenAccount : Command
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string Owner { get; set; }
    }
}
