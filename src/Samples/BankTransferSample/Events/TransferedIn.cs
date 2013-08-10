using System;
using ENode.Eventing;

namespace BankTransferSample.Events {
    [Serializable]
    public class TransferedIn : Event {
        public Guid SourceAccountId { get; private set; }
        public Guid TargetAccountId { get; private set; }
        public double Amount { get; private set; }
        public string Description { get; private set; }

        public TransferedIn(Guid sourceAccountId, Guid targetAccountId, double amount, string description) {
            SourceAccountId = sourceAccountId;
            TargetAccountId = targetAccountId;
            Amount = amount;
            Description = description;
        }
    }
}
