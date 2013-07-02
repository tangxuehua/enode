using System;
using System.Reflection;
using System.Threading;
using BankTransferSagaSample.Commands;
using BankTransferSagaSample.Domain;
using ENode;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;

namespace BankTransferSagaSample
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();
            var memoryCache = ObjectContainer.Resolve<IMemoryCache>();

            var bankAccountId1 = Guid.NewGuid();
            var bankAccountId2 = Guid.NewGuid();

            //开两个银行账户
            var openAccountCommand1 = new OpenAccount { AccountId = bankAccountId1, AccountNumber = "00001", Owner = "雪华" };
            var openAccountCommand2 = new OpenAccount { AccountId = bankAccountId2, AccountNumber = "00002", Owner = "凯锋" };
            commandService.Execute(openAccountCommand1);
            commandService.Execute(openAccountCommand2);

            //每个账户都存入1000元
            var depositCommand1 = new Deposit { AccountId = bankAccountId1, Amount = 1000 };
            var depositCommand2 = new Deposit { AccountId = bankAccountId2, Amount = 1000 };
            commandService.Execute(depositCommand1);
            commandService.Execute(depositCommand2);

            //账户1向账户2转账300元
            commandService.Send(new StartTransfer { TransferInfo = new TransferInfo(bankAccountId1, bankAccountId2, 300) });
            Thread.Sleep(1000);

            //账户2向账户1转账500元
            commandService.Send(new StartTransfer { TransferInfo = new TransferInfo(bankAccountId2, bankAccountId1, 500) });
            Thread.Sleep(1000);

            //从内存获取账户信息，检查余额是否正确
            var bankAccount1 = memoryCache.Get<BankAccount>(bankAccountId1.ToString());
            var bankAccount2 = memoryCache.Get<BankAccount>(bankAccountId2.ToString());

            Console.WriteLine(string.Format("账户{0}余额:{1}", bankAccount1.AccountNumber, bankAccount1.Balance));
            Console.WriteLine(string.Format("账户{0}余额:{1}", bankAccount2.AccountNumber, bankAccount2.Balance));

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        static void InitializeENodeFramework()
        {
            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };

            //全部使用默认配置，一般单元测试时，可以使用该配置

            Configuration
                .Create()
                .UseTinyObjectContainer()
                .RegisterAllDefaultFrameworkComponents()
                .UseLog4Net("log4net.config")
                .UseDefaultCommandHandlerProvider(assemblies)
                .UseDefaultAggregateRootTypeProvider(assemblies)
                .UseDefaultAggregateRootInternalHandlerProvider(assemblies)
                .UseDefaultEventHandlerProvider(assemblies)
                .UseDefaultEventPersistenceSynchronizerProvider(assemblies)
                .UseAllDefaultProcessors(
                    new string[] { "CommandQueue" },
                    "RetryCommandQueue",
                    new string[] { "EventQueue" })
                .Start();
        }
    }
}
