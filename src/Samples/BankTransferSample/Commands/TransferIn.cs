using System;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    [Serializable]
    public class TransferIn : Command
    {
        public Guid SourceAccountId { get; set; }
        public Guid TargetAccountId { get; set; }
        public double Amount { get; set; }
    }
}
