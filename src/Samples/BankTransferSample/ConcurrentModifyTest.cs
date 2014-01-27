using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using BankTransferSample.Commands;
using BankTransferSample.Domain.BankAccounts;
using BankTransferSample.DomainEvents.BankAccount;
using BankTransferSample.DomainEvents.Transaction;
using ENode;
using ECommon.Autofac;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ENode.Commanding;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;


namespace BankTransferSample
{
    //public class ConcurrentModifyTest
    //{
    //    public static ManualResetEvent Signal;
    //    public static long TotalCount = 3000;
    //    public static long FinishedCount = 0;

    //    public static void Run()
    //    {
    //        InitializeENodeFramework();

    //        var commandService = ObjectContainer.Resolve<ICommandService>();
    //        var memoryCache = ObjectContainer.Resolve<IMemoryCache>();

    //        //先创建一个银行账户
    //        Signal = new ManualResetEvent(false);
    //        var createAccountCommand = new CreateAccount("00001", "雪华");
    //        commandService.Send(createAccountCommand);
    //        Signal.WaitOne();

    //        //执行N次存款操作，测试高并发修改的处理能力
    //        Signal = new ManualResetEvent(false);
    //        var watch = Stopwatch.StartNew();
    //        for (var index = 0; index < TotalCount; index++)
    //        {
    //            commandService.Send(new Deposit("00001", 1));
    //        }
    //        Signal.WaitOne();

    //        Console.WriteLine("{0} deposit commands completed, total time spent:{1}ms", TotalCount, watch.ElapsedMilliseconds);
    //        var sourceAccount = memoryCache.Get<BankAccount>("00001");
    //        Console.WriteLine("Account current balance:{0}", sourceAccount.Balance);
    //        Console.ReadLine();
    //    }

    //    static void InitializeENodeFramework()
    //    {
    //        var assemblies = new[] { Assembly.GetExecutingAssembly() };

    //        Configuration
    //            .Create()
    //            .UseAutofac()
    //            .RegisterFrameworkComponents()
    //            .RegisterBusinessComponents(assemblies)
    //            .UseLog4Net()
    //            .UseJsonNet()
    //            .CreateAllDefaultProcessors()
    //            .Initialize(assemblies)
    //            .Start();
    //    }
    //}

    //[Component(LifeStyle.Singleton)]
    //public class ConcurrentModifyTestSyncService : IEventHandler<AccountCreated>, IEventHandler<Deposited>
    //{
    //    public void Handle(AccountCreated evnt)
    //    {
    //        ConcurrentModifyTest.Signal.Set();
    //    }
    //    public void Handle(Deposited evnt)
    //    {
    //        var result = Interlocked.Increment(ref ConcurrentModifyTest.FinishedCount);
    //        if (result == ConcurrentModifyTest.TotalCount)
    //        {
    //            ConcurrentModifyTest.Signal.Set();
    //        }
    //    }
    //}
}
