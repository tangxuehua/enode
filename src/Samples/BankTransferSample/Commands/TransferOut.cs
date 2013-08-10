using System;
using ENode.Commanding;

namespace BankTransferSample.Commands {
    [Serializable]
    public class TransferOut : Command {
        public Guid SourceAccountId { get; set; }
        public Guid TargetAccountId { get; set; }
        public long Amount { get; set; }
    }
}
