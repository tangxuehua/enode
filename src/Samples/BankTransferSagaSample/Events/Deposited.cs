using System;
using ENode.Eventing;

namespace BankTransferSagaSample.Events {
    /// <summary>钱已存入
    /// </summary>
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
