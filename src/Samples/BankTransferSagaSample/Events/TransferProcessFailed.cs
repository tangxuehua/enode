using System;
using ENode.Eventing;

namespace BankTransferSagaSample.Events
{
    [Serializable]
    public class TransferProcessFailed : Event
    {
        public Guid ProcessId { get; private set; }
        public string ErrorMessage { get; private set; }

        public TransferProcessFailed(Guid processId, string errorMessage)
        {
            ProcessId = processId;
            ErrorMessage = errorMessage;
        }
    }
}
