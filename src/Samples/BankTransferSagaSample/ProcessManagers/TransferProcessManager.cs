using System;
using BankTransferSagaSample.Commands;
using BankTransferSagaSample.Events;
using ENode.Commanding;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSagaSample.ProcessManagers
{
    /// <summary>银行转账流程管理器，用于协调银行转账流程中各个参与者聚合根之间的消息交互。
    /// </summary>
    [Component]
    public class TransferProcessManager :
        IEventHandler<TransferProcessStarted>,       //转账流程已发起
        IEventHandler<TransferOutRequested>,         //转出的请求已发起
        IEventHandler<TransferedOut>,                //钱已转出
        IEventHandler<TransferInRequested>,          //转入的请求已发起
        IEventHandler<TransferedIn>,                 //钱已转入
        IEventHandler<RollbackTransferOutRequested>, //回滚转出的请求已发起
        IEventHandler<TransferOutRolledback>,        //转出已回滚
        IEventHandler<TransferProcessCompleted>      //转账流程已完成
    {
        private readonly ICommandService _commandService;

        public TransferProcessManager(ICommandService commandService)
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
                if (result.ErrorInfo != null)
                {
                    _commandService.Send(new HandleFailedTransferOut(evnt.ProcessId) { TransferInfo = evnt.TransferInfo, ErrorInfo = result.ErrorInfo });
                }
            });
        }
        void IEventHandler<TransferedOut>.Handle(TransferedOut evnt)
        {
            Console.WriteLine(evnt.Description);
            //响应已转出事件，发送“处理已转出事件”的命令
            _commandService.Send(new HandleTransferedOut(evnt.ProcessId) { TransferInfo = evnt.TransferInfo });
        }
        void IEventHandler<TransferInRequested>.Handle(TransferInRequested evnt)
        {
            //响应“转入的命令请求已发起”这个事件，发送“转入”命令
            _commandService.Send(new TransferIn(evnt.ProcessId) { TransferInfo = evnt.TransferInfo }, result =>
            {
                //这里是command的异步回调函数，如果有异常，则发送“处理转入失败”的命令
                if (result.ErrorInfo != null)
                {
                    _commandService.Send(new HandleFailedTransferIn(evnt.ProcessId) { TransferInfo = evnt.TransferInfo, ErrorInfo = result.ErrorInfo });
                }
            });
        }
        void IEventHandler<TransferedIn>.Handle(TransferedIn evnt)
        {
            Console.WriteLine(evnt.Description);
            //响应已转入事件，发送“处理已转入事件”的命令
            _commandService.Send(new HandleTransferedIn(evnt.ProcessId) { TransferInfo = evnt.TransferInfo });
        }
        void IEventHandler<RollbackTransferOutRequested>.Handle(RollbackTransferOutRequested evnt)
        {
            //响应“回滚转出的命令请求已发起”这个事件，发送“回滚转出”命令
            _commandService.Send(new RollbackTransferOut(evnt.ProcessId) { TransferInfo = evnt.TransferInfo });
        }
        void IEventHandler<TransferOutRolledback>.Handle(TransferOutRolledback evnt)
        {
            Console.WriteLine(evnt.Description);
            //响应转出已回滚事件，发送“处理转出已回滚事件”的命令
            _commandService.Send(new HandleTransferOutRolledback(evnt.ProcessId) { TransferInfo = evnt.TransferInfo });
        }
        void IEventHandler<TransferProcessCompleted>.Handle(TransferProcessCompleted evnt)
        {
            Console.WriteLine(evnt.ProcessResult.IsSuccess ? "转账流程已顺利完成！" : string.Format("转账失败，错误信息：{0}", evnt.ProcessResult.ErrorInfo));
        }
    }
}
