using System;
namespace BankTransferSample.Domain
{
    /// <summary>转账信息值对象，包含了转账的基本信息
    /// </summary>
    [Serializable]
    public class TransferInfo
    {
        /// <summary>源账号
        /// </summary>
        public string SourceAccountId { get; private set; }
        /// <summary>目标账号
        /// </summary>
        public string TargetAccountId { get; private set; }
        /// <summary>转账金额
        /// </summary>
        public double Amount { get; private set; }

        public TransferInfo(string sourceAccountId, string targetAccountId, double amount)
        {
            SourceAccountId = sourceAccountId;
            TargetAccountId = targetAccountId;
            Amount = amount;
        }
    }
}
