using System;
using BankTransferSagaSample.Commands;
using BankTransferSagaSample.Events;
using ENode.Commanding;
using ENode.Eventing;

namespace BankTransferSagaSample.EventHandlers
{
    public class TransferProcessManagerEventHandler :
        IEventHandler<TransferProcessStarted>,
        IEventHandler<TransferOutRequested>,
        IEventHandler<TransferInRequested>,
        IEventHandler<TransferOutFailHandled>,
        IEventHandler<TransferInFailHandled>,
        IEventHandler<RollbackTransferOutRequested>
    {
        private ICommandService _commandService;

        public TransferProcessManagerEventHandler(ICommandService commandService)
        {
            _commandService = commandService;
        }

        void IEventHandler<TransferProcessStarted>.Handle(TransferProcessStarted evnt)
        {
            Console.WriteLine(evnt.Description);
        }
        void IEventHandler<TransferOutRequested>.Handle(TransferOutRequested evnt)
        {
            _commandService.Send(new TransferOut { ProcessId = evnt.ProcessId, TransferInfo = evnt.TransferInfo }, (result) =>
            {
                if (result.Exception != null)
                {
                    _commandService.Send(new HandleTransferOutFail { ProcessId = evnt.ProcessId, TransferInfo = evnt.TransferInfo, ErrorMessage = result.Exception.Message });
                }
            });
        }
        void IEventHandler<TransferInRequested>.Handle(TransferInRequested evnt)
        {
            _commandService.Send(new TransferIn { ProcessId = evnt.ProcessId, TransferInfo = evnt.TransferInfo }, (result) =>
            {
                if (result.Exception != null)
                {
                    _commandService.Send(new HandleTransferInFail { ProcessId = evnt.ProcessId, TransferInfo = evnt.TransferInfo, ErrorMessage = result.Exception.Message });
                }
            });
        }
        void IEventHandler<TransferOutFailHandled>.Handle(TransferOutFailHandled evnt)
        {
            Console.WriteLine(evnt.ErrorMessage);
        }
        void IEventHandler<TransferInFailHandled>.Handle(TransferInFailHandled evnt)
        {
            Console.WriteLine(evnt.ErrorMessage);
        }
        void IEventHandler<RollbackTransferOutRequested>.Handle(RollbackTransferOutRequested evnt)
        {
            _commandService.Send(new RollbackTransferOut { ProcessId = evnt.ProcessId, TransferInfo = evnt.TransferInfo });
        }
    }
}
