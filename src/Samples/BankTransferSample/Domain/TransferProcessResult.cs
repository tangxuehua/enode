using System;
using ENode.Infrastructure;

namespace BankTransferSagaSample.Domain
{
    /// <summary>值对象，包含了转账流程的结果信息
    /// </summary>
    [Serializable]
    public class TransferProcessResult
    {
        private static readonly TransferProcessResult SuccessResult = new TransferProcessResult(true, null);

        /// <summary>转账是否成功
        /// </summary>
        public bool IsSuccess { get; private set; }
        /// <summary>错误信息
        /// </summary>
        public ErrorInfo ErrorInfo { get; private set; }

        public TransferProcessResult(bool isSuccess, ErrorInfo errorInfo)
        {
            IsSuccess = isSuccess;
            ErrorInfo = errorInfo;
        }

        /// <summary>表示转账成功的结果
        /// </summary>
        public static TransferProcessResult Success { get { return SuccessResult; } }
    }
}
