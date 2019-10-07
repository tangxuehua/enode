using System.Collections.Generic;
using ENode.Domain;

namespace BankTransferSample.Domain
{
    public class InsufficientBalanceException : DomainException
    {
        /// <summary>账户ID
        /// </summary>
        public string AccountId { get; private set; }
        /// <summary>交易ID
        /// </summary>
        public string TransactionId { get; private set; }
        /// <summary>交易类型
        /// </summary>
        public TransactionType TransactionType { get; private set; }
        /// <summary>交易金额
        /// </summary>
        public double Amount { get; private set; }
        /// <summary>当前余额
        /// </summary>
        public double CurrentBalance { get; private set; }
        /// <summary>当前可用余额
        /// </summary>
        public double CurrentAvailableBalance { get; private set; }

        public InsufficientBalanceException(string accountId, string transactionId, TransactionType transactionType, double amount, double currentBalance, double currentAvailableBalance) : base()
        {
            AccountId = accountId;
            TransactionId = transactionId;
            TransactionType = transactionType;
            Amount = amount;
            CurrentBalance = currentBalance;
            CurrentAvailableBalance = currentAvailableBalance;
        }

        public override void SerializeTo(IDictionary<string, string> serializableInfo)
        {
            serializableInfo.Add("AccountId", AccountId);
            serializableInfo.Add("TransactionId", TransactionId);
            serializableInfo.Add("TransactionType", ((int)TransactionType).ToString());
            serializableInfo.Add("Amount", Amount.ToString());
            serializableInfo.Add("CurrentBalance", CurrentBalance.ToString());
            serializableInfo.Add("CurrentAvailableBalance", CurrentAvailableBalance.ToString());
        }
        public override void RestoreFrom(IDictionary<string, string> serializableInfo)
        {
            AccountId = serializableInfo["AccountId"];
            TransactionId = serializableInfo["TransactionId"];
            TransactionType = (TransactionType)int.Parse(serializableInfo["TransactionType"]);
            Amount = double.Parse(serializableInfo["Amount"]);
            CurrentBalance = double.Parse(serializableInfo["CurrentBalance"]);
            CurrentAvailableBalance = double.Parse(serializableInfo["CurrentAvailableBalance"]);
        }
    }
}
