using BankTransferSample.Exceptions;
using ENode.Exceptions;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    public class ExceptionTypeCodeProvider : AbstractTypeCodeProvider<IPublishableException>, ITypeCodeProvider<IPublishableException>
    {
        public ExceptionTypeCodeProvider()
        {
            RegisterType<InsufficientBalanceException>(100);
        }
    }
}
