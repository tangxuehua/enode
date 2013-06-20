using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events
{
    [Serializable]
    public class TransferInFailHandled : AbstractTransferEvent
    {
        public string ErrorMessage { get; private set; }

        public TransferInFailHandled(Guid processId, TransferInfo transferInfo, string errorMessage)
            : base(processId, transferInfo)
        {
            ErrorMessage = errorMessage;
        }
    }
}
