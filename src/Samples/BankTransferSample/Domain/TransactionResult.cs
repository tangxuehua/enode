using System;
using ENode.Infrastructure;

namespace BankTransferSample.Domain
{
    /// <summary>值对象，包含了转账交易的结果信息
    /// </summary>
    [Serializable]
    public class TransactionResult
    {
        private static readonly TransactionResult SuccessResult = new TransactionResult(true, null);

        /// <summary>转账是否成功
        /// </summary>
        public bool IsSuccess { get; private set; }
        /// <summary>错误信息
        /// </summary>
        public ErrorInfo ErrorInfo { get; private set; }

        public TransactionResult(bool isSuccess, ErrorInfo errorInfo)
        {
            IsSuccess = isSuccess;
            ErrorInfo = errorInfo;
        }

        /// <summary>表示转账成功的结果
        /// </summary>
        public static TransactionResult Success
        {
            get { return SuccessResult; }
        }
    }
}
