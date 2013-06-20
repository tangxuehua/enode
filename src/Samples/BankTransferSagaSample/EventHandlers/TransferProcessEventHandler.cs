using System;
using BankTransferSagaSample.Commands;
using BankTransferSagaSample.Events;
using ENode.Commanding;
using ENode.Eventing;

namespace BankTransferSagaSample.EventHandlers
{
    /// <summary>事件订阅者，用于监听和响应转账流程聚合根产生的事件
    /// </summary>
    public class TransferProcessEventHandler :
        IEventHandler<TransferProcessStarted>,       //转账流程已开始
        IEventHandler<TransferOutRequested>,         //转出的请求已发起
        IEventHandler<TransferInRequested>,          //转入的请求已发起
        IEventHandler<RollbackTransferOutRequested>, //回滚转出的请求已发起
        IEventHandler<TransferProcessCompleted>,     //转账流程已完成
        IEventHandler<TransferProcessAborted>        //转账流程已终止
    {
        private ICommandService _commandService;

        public TransferProcessEventHandler(ICommandService commandService)
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
                    Console.WriteLine(result.Exception.Message);
                    _commandService.Send(new HandleFailedTransferOut { ProcessId = evnt.ProcessId, TransferInfo = evnt.TransferInfo });
                }
            });
        }
        void IEventHandler<TransferInRequested>.Handle(TransferInRequested evnt)
        {
            _commandService.Send(new TransferIn { ProcessId = evnt.ProcessId, TransferInfo = evnt.TransferInfo }, (result) =>
            {
                if (result.Exception != null)
                {
                    Console.WriteLine(result.Exception.Message);
                    _commandService.Send(new HandleFailedTransferIn { ProcessId = evnt.ProcessId, TransferInfo = evnt.TransferInfo });
                }
            });
        }
        void IEventHandler<RollbackTransferOutRequested>.Handle(RollbackTransferOutRequested evnt)
        {
            _commandService.Send(new RollbackTransferOut { ProcessId = evnt.ProcessId, TransferInfo = evnt.TransferInfo });
        }
        void IEventHandler<TransferProcessCompleted>.Handle(TransferProcessCompleted evnt)
        {
            Console.WriteLine("转账流程已正常完成！");
        }
        void IEventHandler<TransferProcessAborted>.Handle(TransferProcessAborted evnt)
        {
            Console.WriteLine("转账流程已异常终止！");
        }
    }
}
