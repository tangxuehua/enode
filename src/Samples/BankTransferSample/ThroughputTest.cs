using System;
using System.Diagnostics;
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
    //public class ThroughputTest
    //{
    //    public static ManualResetEvent Signal = new ManualResetEvent(false);
    //    public static long TotalTransactions = 100000;
    //    public static long TotalFinishedTransactions = 0;

    //    public static void Run()
    //    {
    //        InitializeENodeFramework();

    //        var commandService = ObjectContainer.Resolve<ICommandService>();

    //        ////创建两个银行账户
    //        //var createAccountCommand1 = new CreateAccount("00001", "雪华");
    //        //var createAccountCommand2 = new CreateAccount("00002", "凯锋");
    //        //commandService.Send(createAccountCommand1);
    //        //commandService.Send(createAccountCommand2);
    //        //Signal.WaitOne();

    //        Configuration.Instance.Total = TotalTransactions;
    //        var watch = Configuration.Instance.TotalWatch;

    //        watch.Start();
    //        //Configuration.Instance.CommandWatch.Start();
    //        //Configuration.Instance.UnCommittedWatch.Start();
    //        //Configuration.Instance.CommittedWatch.Start();
    //        for (var index = 0; index < TotalTransactions; index++)
    //        {
    //            commandService.Send(new CreateAccount(index.ToString(), "雪华"));
    //        }
    //        Console.WriteLine("Command Send Completed:" + watch.ElapsedMilliseconds);
    //        Signal.WaitOne();

    //        //Thread.Sleep(100);
    //        //Console.WriteLine(string.Empty);

    //        ////每个账户都存入10000000元
    //        //Signal = new ManualResetEvent(false);
    //        //var depositCommand1 = new Deposit("00001", 10000000);
    //        //var depositCommand2 = new Deposit("00002", 10000000);
    //        //commandService.Send(depositCommand1);
    //        //commandService.Send(depositCommand2);
    //        //Signal.WaitOne();

    //        //Thread.Sleep(100);
    //        //Console.WriteLine(string.Empty);

    //        //账户1向账户2转账1元
    //        //Signal = new ManualResetEvent(false);
    //        //CreateTransaction transactionCommand;
    //        //var watch = Stopwatch.StartNew();
    //        //for (var index = 0; index < TotalTransactions; index++)
    //        //{
    //        //    transactionCommand = new CreateTransaction(new TransactionInfo(index.ToString(), (-1 * index).ToString(), 1D));
    //        //    commandService.Send(transactionCommand);
    //        //}
    //        //Signal.WaitOne();

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
    //public class ThroughputTestSyncService :
    //    IEventHandler<AccountCreated>
    //    //IEventHandler<Deposited>,
    //    //IEventHandler<TransactionCompleted>
    //{
    //    private bool _account1Created;
    //    private bool _account2Created;

    //    private bool _account1Deposited;
    //    private bool _account2Deposited;

    //    public void Handle(AccountCreated evnt)
    //    {
    //        ThroughputTest.TotalFinishedTransactions++;
    //        if (ThroughputTest.TotalFinishedTransactions == ThroughputTest.TotalTransactions)
    //        {
    //            ThroughputTest.Signal.Set();
    //        }

    //        //if (evnt.SourceId == "00001")
    //        //{
    //        //    _account1Created = true;
    //        //}
    //        //else if (evnt.SourceId == "00002")
    //        //{
    //        //    _account2Created = true;
    //        //}

    //        //if (_account1Created && _account2Created)
    //        //{
    //        //    ThroughputTest.Signal.Set();
    //        //}
    //    }
    //    public void Handle(Deposited evnt)
    //    {
    //        if (evnt.SourceId == "00001")
    //        {
    //            _account1Deposited = true;
    //        }
    //        else if (evnt.SourceId == "00002")
    //        {
    //            _account2Deposited = true;
    //        }

    //        if (_account1Deposited && _account2Deposited)
    //        {
    //            ThroughputTest.Signal.Set();
    //        }
    //    }
    //    public void Handle(TransactionCompleted evnt)
    //    {
    //        //ThroughputTest.TotalFinishedTransactions++;
    //        //Console.WriteLine(ThroughputTest.TotalFinishedTransactions);
    //        //if (ThroughputTest.TotalFinishedTransactions == ThroughputTest.TotalTransactions)
    //        //{
    //        //    ThroughputTest.Signal.Set();
    //        //}
    //    }
    //}
    ////[Component(LifeStyle.Singleton)]
    ////public class ThroughputTestEventLogger :
    ////    IEventHandler<AccountCreated>,                      //账号已创建
    ////    IEventHandler<Deposited>                            //已存款
    ////{
    ////    private readonly ICommandService _commandService;

    ////    public ThroughputTestEventLogger(ICommandService commandService)
    ////    {
    ////        _commandService = commandService;
    ////    }

    ////    public void Handle(AccountCreated evnt)
    ////    {
    ////        Console.WriteLine("账号已创建，账号：{0}，所有者：{1}", evnt.SourceId, evnt.Owner);
    ////    }
    ////    public void Handle(Deposited evnt)
    ////    {
    ////        Console.WriteLine("存款已成功，账号：{0}，金额：{1}，当前余额：{2}", evnt.SourceId, evnt.Amount, evnt.CurrentBalance);
    ////    }
    ////}
}
