using System;

namespace BankTransferSample.Domain.BankAccounts
{
    /// <summary>实体，表示一笔转账交易中的预转入信息
    /// </summary>
    public class CreditPreparation
    {
        public Guid TransactionId { get; private set; }
        public double Amount { get; private set; }

        public CreditPreparation(Guid transactionId, double amount)
        {
            TransactionId = transactionId;
            Amount = amount;
        }
    }
}
