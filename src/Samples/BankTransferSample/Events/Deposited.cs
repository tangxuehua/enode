using System;
using ENode.Eventing;

namespace BankTransferSample.Events {
    [Serializable]
    public class Deposited : Event {
        public Guid AccountId { get; private set; }
        public double Amount { get; private set; }
        public string Description { get; private set; }

        public Deposited(Guid accountId, double amount, string description) {
            AccountId = accountId;
            Amount = amount;
            Description = description;
        }
    }
}
