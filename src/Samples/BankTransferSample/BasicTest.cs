using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BankTransferSample.Commands;
using BankTransferSample.DomainEvents.BankAccount;
using BankTransferSample.DomainEvents.Transaction;
using BankTransferSample.EQueueIntegrations;
using ECommon.Autofac;
using ECommon.Configurations;
using ECommon.IoC;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Eventing;

namespace BankTransferSample
{
    public class BasicTest
    {
        public static void Run()
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();

            //创建两个银行账户
            var createAccountCommand1 = new CreateAccount("00001", "雪华");
            var createAccountCommand2 = new CreateAccount("00002", "凯锋");
            var task1 = commandService.Send(createAccountCommand1);
            var task2 = commandService.Send(createAccountCommand2);
            Task.WaitAll(task1, task2);

            Thread.Sleep(500);
            Console.WriteLine(string.Empty);

            //每个账户都存入1000元
            var depositCommand1 = new Deposit("00001", 1000);
            var depositCommand2 = new Deposit("00002", 1000);
            task1 = commandService.Send(depositCommand1);
            task2 = commandService.Send(depositCommand2);
            Task.WaitAll(task1, task2);

            Thread.Sleep(500);
            Console.WriteLine(string.Empty);

            //账户1向账户2转账300元
            commandService.Send(new CreateTransaction(new TransactionInfo(Guid.NewGuid(), "00001", "00002", 300D))).Wait();
            //账户2向账户1转账500元
            commandService.Send(new CreateTransaction(new TransactionInfo(Guid.NewGuid(), "00002", "00001", 500D))).Wait();

            Thread.Sleep(500);
            Console.WriteLine(string.Empty);
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        static void InitializeENodeFramework()
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseAutofac()
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .UseLog4Net()
                .UseJsonNet()
                .UseEQueue()
                .InitializeENode(assemblies)
                .StartEnode()
                .StartEQueue();
        }
    }

    [Component]
    public class BasicTestEventLogger :
        IEventHandler<AccountCreated>,                      //账号已创建
        IEventHandler<Deposited>,                           //已存款
        IEventHandler<Withdrawn>,                           //已取款
        IEventHandler<TransactionStarted>,                  //交易已开始
        IEventHandler<DebitPrepared>,                       //交易已预转出
        IEventHandler<CreditPrepared>,                      //交易已预转入
        IEventHandler<DebitInsufficientBalance>,            //余额不足不允许预转出操作
        IEventHandler<DebitPreparationConfirmed>,           //交易预转出已确认
        IEventHandler<CreditPreparationConfirmed>,          //交易预转入已确认
        IEventHandler<TransactionCommitted>,                //交易已提交
        IEventHandler<DebitCommitted>,                      //交易转出已提交
        IEventHandler<CreditCommitted>,                     //交易转入已提交
        IEventHandler<DebitAborted>,                        //交易转出已终止
        IEventHandler<CreditAborted>,                       //交易转入已终止
        IEventHandler<DebitConfirmed>,                      //交易转出已确认
        IEventHandler<CreditConfirmed>,                     //交易转入已确认
        IEventHandler<TransactionCompleted>,                //交易已完成
        IEventHandler<TransactionAborted>                   //交易已终止
    {
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
        public void Handle(DebitInsufficientBalance evnt)
        {
            Console.WriteLine("余额不足不允许预转出操作，交易ID：{0}，账号：{1}，金额：{2}，当前余额：{3}，当前可用余额：{4}", evnt.TransactionId, evnt.SourceId, evnt.Amount, evnt.CurrentBalance, evnt.CurrentAvailableBalance);
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
            Console.WriteLine("交易转出已提交，交易ID：{0}，账号：{1}，金额：{2}，当前余额：{3}", evnt.TransactionId, evnt.SourceId, evnt.Amount, evnt.CurrentBalance);
        }
        public void Handle(CreditCommitted evnt)
        {
            Console.WriteLine("交易转入已提交，交易ID：{0}，账号：{1}，金额：{2}，当前余额：{3}", evnt.TransactionId, evnt.SourceId, evnt.Amount, evnt.CurrentBalance);
        }
        public void Handle(DebitAborted evnt)
        {
            Console.WriteLine("交易转出已终止，交易ID：{0}，账号：{1}，金额：{2}", evnt.TransactionId, evnt.SourceId, evnt.Amount);
        }
        public void Handle(CreditAborted evnt)
        {
            Console.WriteLine("交易转入已终止，交易ID：{0}，账号：{1}，金额：{2}", evnt.TransactionId, evnt.SourceId, evnt.Amount);
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
            Console.WriteLine(string.Empty);
        }
        public void Handle(TransactionAborted evnt)
        {
            Console.WriteLine("交易已终止，交易ID：{0}", evnt.SourceId);
            Console.WriteLine(string.Empty);
        }
    }
}
