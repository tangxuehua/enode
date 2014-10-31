using BankTransferSample.Domain;
using ECommon.Components;
using ENode.Domain;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    [Component(LifeStyle.Singleton)]
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
