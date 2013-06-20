using System;
using BankTransferSagaSample.Domain;

namespace BankTransferSagaSample.Events
{
    [Serializable]
    public class TransferOutFailHandled : AbstractTransferEvent
    {
        public string ErrorMessage { get; private set; }

        public TransferOutFailHandled(Guid processId, TransferInfo transferInfo, string errorMessage)
            : base(processId, transferInfo)
        {
            ErrorMessage = errorMessage;
        }
    }
}
