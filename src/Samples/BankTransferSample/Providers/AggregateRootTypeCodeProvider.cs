using BankTransferSample.Domain;
using ENode.Domain;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    public class AggregateRootTypeCodeProvider : AbstractTypeCodeProvider, IAggregateRootTypeCodeProvider
    {
        public AggregateRootTypeCodeProvider()
        {
            RegisterType<BankAccount>(100);
            RegisterType<DepositTransaction>(101);
            RegisterType<TransferTransaction>(102);
        }
    }
}
