using System;
using System.Reflection;
using System.Threading;
using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode;
using ENode.Autofac;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.JsonNet;
using ENode.Log4Net;

namespace BankTransferSample
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();
            var memoryCache = ObjectContainer.Resolve<IMemoryCache>();

            //开两个银行账户
            var openAccountCommand1 = new OpenAccount { AccountId = "00001", Owner = "雪华" };
            var openAccountCommand2 = new OpenAccount { AccountId = "00002", Owner = "凯锋" };
            commandService.Execute(openAccountCommand1);
            commandService.Execute(openAccountCommand2);

            //每个账户都存入1000元
            var depositCommand1 = new Deposit { AccountId = "00001", Amount = 1000 };
            var depositCommand2 = new Deposit { AccountId = "00002", Amount = 1000 };
            commandService.Execute(depositCommand1);
            commandService.Execute(depositCommand2);

            //账户1向账户2转账300元
            commandService.Send(new StartTransfer { TransferInfo = new TransactionInfo("00001", "00002", 300) });
            Thread.Sleep(1000);

            //账户2向账户1转账500元
            commandService.Send(new StartTransfer { TransferInfo = new TransactionInfo("00002", "00001", 500) });
            Thread.Sleep(1000);

            //从内存获取账户信息，检查余额是否正确
            var bankAccount1 = memoryCache.Get<BankAccount>("00001");
            var bankAccount2 = memoryCache.Get<BankAccount>("00002");

            Console.WriteLine("账户{0}余额:{1}", "00001", bankAccount1.Balance);
            Console.WriteLine("账户{0}余额:{1}", "00002", bankAccount2.Balance);

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
}
