using System;
using BankTransferSample.DomainEvents.BankAccount;
using BankTransferSample.DomainEvents.Transaction;
using ENode.Commanding;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.ProcessManagers
{
    /// <summary>一个日志记录器，记录领域内产生的部分事件。
    /// <remarks>
    /// 目前用于检查银行账号的各种交易是否成功，如存款、取款、转账等交易。
    /// </remarks>
    /// </summary>
    [Component]
    public class EventLogger :
        IEventHandler<AccountCreated>,                      //账号已创建
        IEventHandler<Deposited>,                           //已存款
        IEventHandler<Withdrawn>,                           //已取款
        IEventHandler<TransactionStarted>,                  //交易已开始
        IEventHandler<DebitPrepared>,                       //交易已预转出
        IEventHandler<CreditPrepared>,                      //交易已预转入
        IEventHandler<DebitPreparationConfirmed>,           //交易预转出已确认
        IEventHandler<CreditPreparationConfirmed>,          //交易预转入已确认
        IEventHandler<TransactionCommitted>,                //交易已提交
        IEventHandler<DebitCommitted>,                      //交易转出已提交
        IEventHandler<CreditCommitted>,                     //交易转入已提交
        IEventHandler<DebitConfirmed>,                      //交易转出已确认
        IEventHandler<CreditConfirmed>,                     //交易转入已确认
        IEventHandler<TransactionCompleted>,                //交易已完成
        IEventHandler<TransactionAborted>                   //交易已终止
    {
        private readonly ICommandService _commandService;

        public EventLogger(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public void Handle(AccountCreated evnt)
        {
            Console.WriteLine("账号已创建，账号：{0}，所有者：{1}", evnt.SourceId, evnt.Owner);
        }
        public void Handle(Deposited evnt)
        {
            Console.WriteLine("存款已成功，账号：{0}，金额：{1}，当前余额：{2}", evnt.SourceId, evnt.Amount, evnt.CurrentBalance);
        }
        public void Handle(Withdrawn evnt)
        {
            Console.WriteLine("取款已成功，账号：{0}，金额：{1}，当前余额：{2}", evnt.SourceId, evnt.Amount, evnt.CurrentBalance);
        }
        public void Handle(TransactionStarted evnt)
        {
            Console.WriteLine("交易已开始，交易ID：{0}，源账号：{1}，目标账号：{2}，转账金额：{3}", evnt.SourceId, evnt.TransactionInfo.SourceAccountId, evnt.TransactionInfo.TargetAccountId, evnt.TransactionInfo.Amount);
        }
        public void Handle(DebitPrepared evnt)
        {
            Console.WriteLine("交易预转出成功，交易ID：{0}，账号：{1}，金额：{2}", evnt.TransactionId, evnt.SourceId, evnt.Amount);
        }
        public void Handle(CreditPrepared evnt)
        {
            Console.WriteLine("交易预转入成功，交易ID：{0}，账号：{1}，金额：{2}", evnt.TransactionId, evnt.SourceId, evnt.Amount);
        }
        public void Handle(DebitPreparationConfirmed evnt)
        {
            Console.WriteLine("交易预转出确认成功，交易ID：{0}", evnt.SourceId);
        }
        public void Handle(CreditPreparationConfirmed evnt)
        {
            Console.WriteLine("交易预转入确认成功，交易ID：{0}", evnt.SourceId);
        }
        public void Handle(TransactionCommitted evnt)
        {
            Console.WriteLine("交易已提交，交易ID：{0}", evnt.SourceId);
        }
        public void Handle(DebitCommitted evnt)
        {
            Console.WriteLine("交易转出成功，交易ID：{0}，账号：{1}，金额：{2}，当前余额：{3}", evnt.TransactionId, evnt.SourceId, evnt.Amount, evnt.CurrentBalance);
        }
        public void Handle(CreditCommitted evnt)
        {
            Console.WriteLine("交易转入成功，交易ID：{0}，账号：{1}，金额：{2}，当前余额：{3}", evnt.TransactionId, evnt.SourceId, evnt.Amount, evnt.CurrentBalance);
        }
        public void Handle(DebitConfirmed evnt)
        {
            Console.WriteLine("交易转出确认成功，交易ID：{0}", evnt.SourceId);
        }
        public void Handle(CreditConfirmed evnt)
        {
            Console.WriteLine("交易转入确认成功，交易ID：{0}", evnt.SourceId);
        }
        public void Handle(TransactionCompleted evnt)
        {
            Console.WriteLine("交易已完成，交易ID：{0}", evnt.SourceId);
        }
        public void Handle(TransactionAborted evnt)
        {
            Console.WriteLine("交易已终止，交易ID：{0}", evnt.SourceId);
        }
    }
}
