using System;
namespace BankTransferSagaSample.Domain
{
    /// <summary>转账信息值对象，包含了转账的基本信息
    /// </summary>
    [Serializable]
    public class TransferInfo
    {
        /// <summary>源账号
        /// </summary>
        public string SourceAccountNumber { get; private set; }
        /// <summary>目标账号
        /// </summary>
        public string TargetAccountNumber { get; private set; }
        /// <summary>转账金额
        /// </summary>
        public double Amount { get; private set; }

        public TransferInfo(string sourceAccountNumber, string targetAccountNumber, double amount)
        {
            SourceAccountNumber = sourceAccountNumber;
            TargetAccountNumber = targetAccountNumber;
            Amount = amount;
        }
    }
}
