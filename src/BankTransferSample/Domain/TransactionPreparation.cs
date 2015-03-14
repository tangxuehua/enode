using System;

namespace BankTransferSample.Domain
{
    /// <summary>实体，表示账户聚合内的一笔预操作（如预存款、预取款、预转入、预转出）
    /// </summary>
    [Serializable]
    public class TransactionPreparation
    {
        /// <summary>账户ID
        /// </summary>
        public string AccountId { get; private set; }
        /// <summary>交易ID
        /// </summary>
        public string TransactionId { get; private set; }
        /// <summary>预借或预贷
        /// </summary>
        public PreparationType PreparationType { get; private set; }
        /// <summary>交易类型
        /// </summary>
        public TransactionType TransactionType { get; private set; }
        /// <summary>交易金额
        /// </summary>
        public double Amount { get; private set; }

        public TransactionPreparation(string accountId, string transactionId, TransactionType transactionType, PreparationType preparationType, double amount)
        {
            if (transactionType == TransactionType.DepositTransaction && preparationType != PreparationType.CreditPreparation)
            {
                throw new MismatchTransactionPreparationException(transactionType, preparationType);
            }
            if (transactionType == TransactionType.WithdrawTransaction && preparationType != PreparationType.DebitPreparation)
            {
                throw new MismatchTransactionPreparationException(transactionType, preparationType);
            }
            AccountId = accountId;
            TransactionId = transactionId;
            TransactionType = transactionType;
            PreparationType = preparationType;
            Amount = amount;
        }
    }
}
