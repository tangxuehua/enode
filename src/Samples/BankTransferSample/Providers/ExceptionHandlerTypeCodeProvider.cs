using BankTransferSample.EventHandlers;
using BankTransferSample.ProcessManagers;
using ECommon.Components;
using ENode.Eventing;
using ENode.Exceptions;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    [Component(LifeStyle.Singleton)]
    public class ExceptionHandlerTypeCodeProvider : AbstractTypeCodeProvider<IExceptionHandler>
    {
        public ExceptionHandlerTypeCodeProvider()
        {
            RegisterType<TransferTransactionProcessManager>(100);
            RegisterType<ConsoleLogger>(101);
        }
    }
}
