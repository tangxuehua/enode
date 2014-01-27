using System;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.Transaction
{
    /// <summary>转账信息值对象，包含了一次转账交易的基本信息
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class TransactionInfo
    {
        /// <summary>转账交易ID
        /// </summary>
        public Guid TransactionId { get; private set; }
        /// <summary>源账号
        /// </summary>
        public string SourceAccountId { get; private set; }
        /// <summary>目标账号
        /// </summary>
        public string TargetAccountId { get; private set; }
        /// <summary>转账金额
        /// </summary>
        public double Amount { get; private set; }

        public TransactionInfo(Guid transactionId, string sourceAccountId, string targetAccountId, double amount)
        {
            TransactionId = transactionId;
            SourceAccountId = sourceAccountId;
            TargetAccountId = targetAccountId;
            Amount = amount;
        }
    }
}
