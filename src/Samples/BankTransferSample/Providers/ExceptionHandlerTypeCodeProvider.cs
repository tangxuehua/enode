using BankTransferSample.EventHandlers;
using BankTransferSample.ProcessManagers;
using ECommon.Components;
using ENode.Exceptions;
using ENode.Infrastructure.Impl;

namespace BankTransferSample.Providers
{
    [Component]
    public class ExceptionHandlerTypeCodeProvider : DefaultTypeCodeProvider<IExceptionHandler>
    {
        public ExceptionHandlerTypeCodeProvider()
        {
            RegisterType<TransferTransactionProcessManager>(100);
            RegisterType<ConsoleLogger>(101);
        }
    }
}
