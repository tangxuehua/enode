using System;
using ENode.Eventing;

namespace BankTransferSagaSample.Events
{
    /// <summary>钱已存入
    /// </summary>
    [Serializable]
    public class Deposited : Event
    {
        public string AccountNumber { get; private set; }
        public double Amount { get; private set; }
        public string Description { get; private set; }

        public Deposited(string accountNumber, double amount, string description)
        {
            AccountNumber = accountNumber;
            Amount = amount;
            Description = description;
        }
    }
}
