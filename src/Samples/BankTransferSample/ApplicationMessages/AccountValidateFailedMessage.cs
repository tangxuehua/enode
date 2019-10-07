using ENode.Messaging;

namespace BankTransferSample.ApplicationMessages
{
    /// <summary>账户验证未通过
    /// </summary>
    public class AccountValidateFailedMessage : ApplicationMessage
    {
        public string AccountId { get; set; }
        public string TransactionId { get; set; }
        public string Reason { get; set; }

        public AccountValidateFailedMessage() { }
        public AccountValidateFailedMessage(string accountId, string transactionId, string reason)
        {
            AccountId = accountId;
            TransactionId = transactionId;
            Reason = reason;
        }
    }
}
