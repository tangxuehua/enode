using System;
using BankTransferSagaSample.Commands;
using BankTransferSagaSample.Events;
using ENode.Commanding;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSagaSample.EventHandlers {
    /// <summary>事件订阅者，用于监听和响应银行账号聚合根产生的事件
    /// </summary>
    [Component]
    public class BankAccountEventHandler :
        IEventHandler<AccountOpened>,         //银行账户已开
        IEventHandler<Deposited>,             //钱已存入
        IEventHandler<TransferedOut>,         //钱已转出
        IEventHandler<TransferedIn>,          //钱已转入
        IEventHandler<TransferOutRolledback>  //转出已回滚
    {
        private ICommandService _commandService;

        public BankAccountEventHandler(ICommandService commandService) {
            _commandService = commandService;
        }

        void IEventHandler<AccountOpened>.Handle(AccountOpened evnt) {
            Console.WriteLine(string.Format("创建银行账户{0}", evnt.AccountNumber));
        }
        void IEventHandler<Deposited>.Handle(Deposited evnt) {
            Console.WriteLine(evnt.Description);
        }
        void IEventHandler<TransferedOut>.Handle(TransferedOut evnt) {
            Console.WriteLine(evnt.Description);
            //响应已转出事件，发送“处理已转出事件”的命令
            _commandService.Send(new HandleTransferedOut(evnt.ProcessId) { TransferInfo = evnt.TransferInfo });
        }
        void IEventHandler<TransferedIn>.Handle(TransferedIn evnt) {
            Console.WriteLine(evnt.Description);
            //响应已转入事件，发送“处理已转入事件”的命令
            _commandService.Send(new HandleTransferedIn(evnt.ProcessId) { TransferInfo = evnt.TransferInfo });
        }
        void IEventHandler<TransferOutRolledback>.Handle(TransferOutRolledback evnt) {
            Console.WriteLine(evnt.Description);
            //响应转出已回滚事件，发送“处理转出已回滚事件”的命令
            _commandService.Send(new HandleTransferOutRolledback(evnt.ProcessId) { TransferInfo = evnt.TransferInfo });
        }
    }
}
