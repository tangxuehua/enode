using BankTransferSample.Exceptions;
using ECommon.Components;
using ENode.Exceptions;
using ENode.Infrastructure.Impl;

namespace BankTransferSample.Providers
{
    [Component]
    public class ExceptionTypeCodeProvider : DefaultTypeCodeProvider<IPublishableException>
    {
        public ExceptionTypeCodeProvider()
        {
            RegisterType<InsufficientBalanceException>(100);
        }
    }
}
