using BankTransferSample.Exceptions;
using ECommon.Components;
using ENode.Exceptions;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    [Component(LifeStyle.Singleton)]
    public class ExceptionTypeCodeProvider : AbstractTypeCodeProvider<IPublishableException>
    {
        public ExceptionTypeCodeProvider()
        {
            RegisterType<InsufficientBalanceException>(100);
        }
    }
}
