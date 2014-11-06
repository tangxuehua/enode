using BankTransferSample.EventHandlers;
using BankTransferSample.ProcessManagers;
using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure.Impl;

namespace BankTransferSample.Providers
{
    [Component]
    public class EventHandlerTypeCodeProvider : DefaultTypeCodeProvider<IEventHandler>
    {
        public EventHandlerTypeCodeProvider()
        {
            RegisterType<DepositTransactionProcessManager>(100);
            RegisterType<TransferTransactionProcessManager>(101);
            RegisterType<ConsoleLogger>(102);
            RegisterType<SyncHelper>(103);
        }
    }
}
