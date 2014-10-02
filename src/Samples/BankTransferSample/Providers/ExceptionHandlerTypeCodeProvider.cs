using BankTransferSample.EventHandlers;
using BankTransferSample.ProcessManagers;
using ENode.Eventing;
using ENode.Exceptions;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    public class ExceptionHandlerTypeCodeProvider : AbstractTypeCodeProvider<IExceptionHandler>, ITypeCodeProvider<IExceptionHandler>
    {
        public ExceptionHandlerTypeCodeProvider()
        {
            RegisterType<TransferTransactionProcessManager>(100);
            RegisterType<ConsoleLogger>(101);
        }
    }
}
