using System;

namespace ENode.Domain
{
    /// <summary>Value object, represents the result of the process.
    /// </summary>
    public class ProcessResult
    {
        private static readonly ProcessResult _successResult = new ProcessResult(true, null);

        /// <summary>Represents whether the process complete successfully.
        /// </summary>
        public bool IsSuccess { get; private set; }
        /// <summary>Represents the error message if the process completed not successfully.
        /// </summary>
        public string ErrorMessage { get; private set; }

        public ProcessResult(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        /// <summary>Represents a success process result.
        /// </summary>
        public static ProcessResult Success { get { return _successResult; } }
    }
}
