using BankTransferSample.Exceptions;
using ECommon.Components;
using ENode.Exceptions;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    [Component]
    public class ExceptionTypeCodeProvider : AbstractTypeCodeProvider<IPublishableException>
    {
        public ExceptionTypeCodeProvider()
        {
            RegisterType<InsufficientBalanceException>(100);
        }
    }
}
