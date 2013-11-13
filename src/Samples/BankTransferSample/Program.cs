using System;
using System.Reflection;
using System.Threading;
using BankTransferSample.Commands;
using BankTransferSample.DomainEvents.BankAccount;
using BankTransferSample.DomainEvents.Transaction;
using ENode;
using ENode.Autofac;
using ENode.Commanding;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.JsonNet;
using ENode.Log4Net;

namespace BankTransferSample
{
    class Program
    {
        public static ManualResetEvent Signal = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();
            var memoryCache = ObjectContainer.Resolve<IMemoryCache>();

            //创建两个银行账户
            var createAccountCommand1 = new CreateAccount("00001", "雪华");
            var createAccountCommand2 = new CreateAccount("00002", "凯锋");
            commandService.Send(createAccountCommand1);
            commandService.Send(createAccountCommand2);
            Signal.WaitOne();

            Thread.Sleep(100);
            Console.WriteLine(string.Empty);

            //每个账户都存入1000元
            Signal = new ManualResetEvent(false);
            var depositCommand1 = new Deposit("00001", 1000);
            var depositCommand2 = new Deposit("00002", 1000);
            commandService.Send(depositCommand1);
            commandService.Send(depositCommand2);
            Signal.WaitOne();

            Thread.Sleep(100);
            Console.WriteLine(string.Empty);

            //账户1向账户2转账300元
            Signal = new ManualResetEvent(false);
            commandService.Send(new CreateTransaction(new TransactionInfo("00001", "00002", 300D)));
            Signal.WaitOne();

            Thread.Sleep(100);
            Console.WriteLine(string.Empty);

            ////账户2向账户1转账500元
            Signal = new ManualResetEvent(false);
            commandService.Send(new CreateTransaction(new TransactionInfo("00002", "00001", 500D)));
            Signal.WaitOne();

            Thread.Sleep(100);
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
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .UseLog4Net()
                .UseJsonNet()
                .CreateAllDefaultProcessors()
                .Initialize(assemblies)
                .Start();
        }
    }

    [Component]
    public class SyncService :
        IEventHandler<AccountCreated>,
        IEventHandler<Deposited>,
        IEventHandler<TransactionCompleted>
    {
        private bool _account1Created;
        private bool _account2Created;

        private bool _account1Deposited;
        private bool _account2Deposited;

        public void Handle(AccountCreated evnt)
        {
            if (evnt.SourceId == "00001")
            {
                _account1Created = true;
            }
            else if (evnt.SourceId == "00002")
            {
                _account2Created = true;
            }

            if (_account1Created && _account2Created)
            {
                Program.Signal.Set();
            }
        }
        public void Handle(Deposited evnt)
        {
            if (evnt.SourceId == "00001")
            {
                _account1Deposited = true;
            }
            else if (evnt.SourceId == "00002")
            {
                _account2Deposited = true;
            }

            if (_account1Deposited && _account2Deposited)
            {
                Program.Signal.Set();
            }
        }
        public void Handle(TransactionCompleted evnt)
        {
            Program.Signal.Set();
        }
    }
}
