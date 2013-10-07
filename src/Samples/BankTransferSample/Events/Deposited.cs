using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>钱已存入
    /// </summary>
    [Serializable]
    public class Deposited : Event
    {
        public double Amount { get; private set; }
        public string Description { get; private set; }

        public Deposited(string accountId, double amount, string description) : base(accountId)
        {
            Amount = amount;
            Description = description;
        }
    }
}
