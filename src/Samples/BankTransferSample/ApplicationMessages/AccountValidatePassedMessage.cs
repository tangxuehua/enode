using ENode.Messaging;

namespace BankTransferSample.ApplicationMessages
{
    /// <summary>账户验证已通过
    /// </summary>
    public class AccountValidatePassedMessage : ApplicationMessage
    {
        public string AccountId { get; set; }
        public string TransactionId { get; set; }

        public AccountValidatePassedMessage() { }
        public AccountValidatePassedMessage(string accountId, string transactionId)
        {
            AccountId = accountId;
            TransactionId = transactionId;
        }
    }
}
