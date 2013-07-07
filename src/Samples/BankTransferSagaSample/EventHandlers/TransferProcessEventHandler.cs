using System;
using BankTransferSagaSample.Commands;
using BankTransferSagaSample.Events;
using ENode.Commanding;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSagaSample.EventHandlers
{
    /// <summary>事件订阅者，用于监听和响应转账流程聚合根产生的事件
    /// </summary>
    [Component]
    public class TransferProcessEventHandler :
        IEventHandler<TransferProcessStarted>,       //转账流程已开始
        IEventHandler<TransferOutRequested>,         //转出的请求已发起
        IEventHandler<TransferInRequested>,          //转入的请求已发起
        IEventHandler<RollbackTransferOutRequested>, //回滚转出的请求已发起
        IEventHandler<TransferProcessCompleted>      //转账流程已完成
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
            //响应“转出的命令请求已发起”这个事件，发送“转出”命令
            _commandService.Send(new TransferOut(evnt.ProcessId) { TransferInfo = evnt.TransferInfo }, (result) =>
            {
                //这里是command的异步回调函数，如果有异常，则发送“处理转出失败”的命令
                if (result.Exception != null)
                {
                    _commandService.Send(
                        new HandleFailedTransferOut(evnt.ProcessId)
                        {
                            TransferInfo = evnt.TransferInfo,
                            Exception = result.Exception
                        });
                }
            });
        }
        void IEventHandler<TransferInRequested>.Handle(TransferInRequested evnt)
        {
            //响应“转入的命令请求已发起”这个事件，发送“转入”命令
            _commandService.Send(new TransferIn(evnt.ProcessId) { TransferInfo = evnt.TransferInfo }, (result) =>
            {
                //这里是command的异步回调函数，如果有异常，则发送“处理转入失败”的命令
                if (result.Exception != null)
                {
                    _commandService.Send(
                        new HandleFailedTransferIn(evnt.ProcessId)
                        {
                            TransferInfo = evnt.TransferInfo,
                            Exception = result.Exception
                        });
                }
            });
        }
        void IEventHandler<RollbackTransferOutRequested>.Handle(RollbackTransferOutRequested evnt)
        {
            //响应“回滚转出的命令请求已发起”这个事件，发送“回滚转出”命令
            _commandService.Send(new RollbackTransferOut(evnt.ProcessId) { TransferInfo = evnt.TransferInfo });
        }
        void IEventHandler<TransferProcessCompleted>.Handle(TransferProcessCompleted evnt)
        {
            if (evnt.ProcessResult.IsSuccess)
            {
                Console.WriteLine("转账流程已顺利完成！");
            }
            else
            {
                Console.WriteLine(string.Format("转账失败，错误信息：{0}", evnt.ProcessResult.ErrorMessage));
            }
        }
    }
}
