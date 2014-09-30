using BankTransferSample.EventHandlers;
using BankTransferSample.ProcessManagers;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    public class EventHandlerTypeCodeProvider : AbstractTypeCodeProvider<IEventHandler>, ITypeCodeProvider<IEventHandler>
    {
        public EventHandlerTypeCodeProvider()
        {
            RegisterType<DepositTransactionProcessManager>(100);
            RegisterType<TransferTransactionProcessManager>(101);
            RegisterType<ConsoleLogger>(102);
        }
    }
}
