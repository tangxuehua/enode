using System;
using ENode.Eventing;

namespace BankTransferSagaSample.Events
{
    [Serializable]
    public class TransferedIn : Event
    {
        public Guid ProcessId { get; private set; }
        public Guid SourceAccountId { get; private set; }
        public Guid TargetAccountId { get; private set; }
        public double Amount { get; private set; }
        public string Description { get; private set; }

        public TransferedIn(Guid processId, Guid sourceAccountId, Guid targetAccountId, double amount, string description)
        {
            ProcessId = processId;
            SourceAccountId = sourceAccountId;
            TargetAccountId = targetAccountId;
            Amount = amount;
            Description = description;
        }
    }
}
