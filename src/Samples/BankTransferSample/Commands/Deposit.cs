using System;
using ENode.Commanding;

namespace BankTransferSample.Commands {
    [Serializable]
    public class Deposit : Command {
        public Guid AccountId { get; set; }
        public long Amount { get; set; }
    }
}
