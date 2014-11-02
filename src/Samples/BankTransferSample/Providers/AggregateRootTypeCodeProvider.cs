using BankTransferSample.Domain;
using ECommon.Components;
using ENode.Domain;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    [Component]
    public class AggregateRootTypeCodeProvider : AbstractTypeCodeProvider<IAggregateRoot>
    {
        public AggregateRootTypeCodeProvider()
        {
            RegisterType<BankAccount>(100);
            RegisterType<DepositTransaction>(101);
            RegisterType<TransferTransaction>(102);
        }
    }
}
