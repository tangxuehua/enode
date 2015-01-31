using BankTransferSample.Domain;
using ECommon.Components;
using ENode.Domain;
using ENode.Infrastructure.Impl;

namespace BankTransferSample.Providers
{
    [Component]
    public class AggregateRootTypeCodeProvider : DefaultTypeCodeProvider<IAggregateRoot>
    {
        public AggregateRootTypeCodeProvider()
        {
            RegisterType<BankAccount>(100);
            RegisterType<DepositTransaction>(101);
            RegisterType<TransferTransaction>(102);
        }
    }
}
