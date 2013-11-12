using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>钱已存入
    /// </summary>
    [Serializable]
    public class Deposited : SourcableDomainEvent<string>
    {
        public double Amount { get; private set; }

        public Deposited(string accountId, double amount) : base(accountId)
        {
            Amount = amount;
        }
    }
}
