using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using BankTransferSample.Commands;
using BankTransferSample.Domain;
using BankTransferSample.EventHandlers;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;

namespace BankTransferSample
{
    class Program
    {
        static ENodeConfiguration _configuration;

        static void Main(string[] args)
        {
            NormalTest();
            //PerformanceTest();
        }

        static void NormalTest()
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            _configuration = Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterAllTypeCodes()
                .RegisterBusinessComponents(assemblies)
                .UseEQueue()
                .InitializeBusinessAssemblies(assemblies)
                .StartEQueue();

            Console.WriteLine(string.Empty);
            Console.WriteLine("ENode started...");

            var commandService = ObjectContainer.Resolve<ICommandService>();
            var syncHelper = ObjectContainer.Resolve<SyncHelper>();
            var account1 = ObjectId.GenerateNewStringId();
            var account2 = ObjectId.GenerateNewStringId();
            var account3 = "INVALID-" + ObjectId.GenerateNewStringId();
            Console.WriteLine(string.Empty);

            //创建两个银行账户
            commandService.ExecuteAsync(new CreateAccountCommand(account1, "雪华"), CommandReturnType.EventHandled).Wait();
            commandService.ExecuteAsync(new CreateAccountCommand(account2, "凯锋"), CommandReturnType.EventHandled).Wait();

            Console.WriteLine(string.Empty);

            //每个账户都存入1000元
            commandService.Send(new StartDepositTransactionCommand(ObjectId.GenerateNewStringId(), account1, 1000));
            syncHelper.WaitOne();
            commandService.Send(new StartDepositTransactionCommand(ObjectId.GenerateNewStringId(), account2, 1000));
            syncHelper.WaitOne();

            Console.WriteLine(string.Empty);

            //账户1向账户3转账300元，交易会失败，因为账户3不存在
            commandService.Send(new StartTransferTransactionCommand(ObjectId.GenerateNewStringId(), new TransferTransactionInfo(account1, account3, 300D)));
            syncHelper.WaitOne();
            Console.WriteLine(string.Empty);

            //账户1向账户2转账1200元，交易会失败，因为余额不足
            commandService.Send(new StartTransferTransactionCommand(ObjectId.GenerateNewStringId(), new TransferTransactionInfo(account1, account2, 1200D)));
            syncHelper.WaitOne();
            Console.WriteLine(string.Empty);

            //账户2向账户1转账500元，交易成功
            commandService.Send(new StartTransferTransactionCommand(ObjectId.GenerateNewStringId(), new TransferTransactionInfo(account2, account1, 500D)));
            syncHelper.WaitOne();

            Thread.Sleep(500);
            Console.WriteLine(string.Empty);
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
            _configuration.ShutdownEQueue();
        }
        static void PerformanceTest()
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            _configuration = Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler()
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterAllTypeCodes()
                .RegisterBusinessComponents(assemblies)
                .UseEQueue()
                .InitializeBusinessAssemblies(assemblies)
                .StartEQueue();

            Console.WriteLine(string.Empty);
            Console.WriteLine("ENode started...");

            var commandService = ObjectContainer.Resolve<ICommandService>();
            var syncHelper = ObjectContainer.Resolve<SyncHelper>();
            var countSyncHelper = ObjectContainer.Resolve<CountSyncHelper>();

            Console.WriteLine(string.Empty);

            var accountList = new List<string>();
            var accountCount = 100;
            var transactionCount = 100;
            var depositAmount = 100000D;
            var transferAmount = 1000D;

            //创建银行账户
            for (var i = 0; i < accountCount; i++)
            {
                var accountId = ObjectId.GenerateNewStringId();
                commandService.ExecuteAsync(new CreateAccountCommand(accountId, "SampleAccount" + i), CommandReturnType.EventHandled).Wait();
                accountList.Add(accountId);
            }

            Console.WriteLine(string.Empty);

            //每个账户都存入初始额度
            foreach (var accountId in accountList)
            {
                commandService.Send(new StartDepositTransactionCommand(ObjectId.GenerateNewStringId(), accountId, depositAmount));
                syncHelper.WaitOne();
            }

            Console.WriteLine(string.Empty);

            countSyncHelper.SetExpectedCount((int)transactionCount);

            var watch = Stopwatch.StartNew();
            for (var i = 0; i < transactionCount; i++)
            {
                var sourceAccountIndex = new Random().Next(accountCount - 1);
                var targetAccountIndex = sourceAccountIndex + 1;
                var sourceAccount = accountList[sourceAccountIndex];
                var targetAccount = accountList[targetAccountIndex];
                commandService.SendAsync(new StartTransferTransactionCommand(ObjectId.GenerateNewStringId(), new TransferTransactionInfo(sourceAccount, targetAccount, transferAmount)));
            }

            countSyncHelper.WaitOne();

            var spentTime = watch.ElapsedMilliseconds;
            Thread.Sleep(500);
            Console.WriteLine(string.Empty);
            Console.WriteLine("All transfer transactions completed, time spent: {0}ms, throughput: {1} transactions per second.", spentTime, transactionCount * 1000 / spentTime);
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
            _configuration.ShutdownEQueue();
        }
    }
}
