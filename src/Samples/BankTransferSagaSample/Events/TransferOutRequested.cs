using System;
using ENode.Eventing;

namespace BankTransferSagaSample.Events
{
    [Serializable]
    public class TransferOutRequested : Event
    {
        public Guid ProcessId { get; private set; }
        public Guid SourceAccountId { get; private set; }
        public Guid TargetAccountId { get; private set; }
        public double Amount { get; private set; }

        public TransferOutRequested(Guid processId, Guid sourceAccountId, Guid targetAccountId, double amount)
        {
            ProcessId = processId;
            SourceAccountId = sourceAccountId;
            TargetAccountId = targetAccountId;
            Amount = amount;
        }
    }
}
