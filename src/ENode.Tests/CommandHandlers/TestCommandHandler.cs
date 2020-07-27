using System;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Messaging;
using ENode.Tests.Commands;
using ENode.Tests.Domain;

namespace ENode.Tests.CommandHandlers
{
    public class TestCommandHandler :
        ICommandHandler<CreateTestAggregateCommand>,
        ICommandHandler<ChangeTestAggregateTitleCommand>,
        ICommandHandler<ChangeTestAggregateTitleWhenDirtyCommand>,
        ICommandHandler<CreateInheritTestAggregateCommand>,
        ICommandHandler<ChangeInheritTestAggregateTitleCommand>,
        ICommandHandler<TestEventPriorityCommand>,
        ICommandHandler<ChangeMultipleAggregatesCommand>,
        ICommandHandler<ChangeNothingCommand>,
        ICommandHandler<ThrowExceptionCommand>,
        ICommandHandler<AggregateThrowExceptionCommand>,
        ICommandHandler<SetResultCommand>
    {
        public Task HandleAsync(ICommandContext context, CreateTestAggregateCommand command)
        {
            if (command.SleepMilliseconds > 0)
            {
                Thread.Sleep(command.SleepMilliseconds);
            }
            context.Add(new TestAggregate(command.AggregateRootId, command.Title));
            return Task.CompletedTask;
        }
        public async Task HandleAsync(ICommandContext context, ChangeTestAggregateTitleCommand command)
        {
            var testAggregate = await context.GetAsync<TestAggregate>(command.AggregateRootId);
            testAggregate.ChangeTitle(command.Title);
        }
        public async Task HandleAsync(ICommandContext context, ChangeTestAggregateTitleWhenDirtyCommand command)
        {
            var testAggregate = await context.GetAsync<TestAggregate>(command.AggregateRootId);
            if (command.IsFirstExecute)
            {
                await ObjectContainer.Resolve<IMemoryCache>().RefreshAggregateFromEventStoreAsync(typeof(TestAggregate).FullName, command.AggregateRootId);
            }
            testAggregate.ChangeTitle(command.Title);
            command.IsFirstExecute = false;
        }
        public Task HandleAsync(ICommandContext context, CreateInheritTestAggregateCommand command)
        {
            context.Add(new InheritTestAggregate(command.AggregateRootId, command.Title));
            return Task.CompletedTask;
        }
        public async Task HandleAsync(ICommandContext context, ChangeInheritTestAggregateTitleCommand command)
        {
            var testAggregate = await context.GetAsync<InheritTestAggregate>(command.AggregateRootId);
            testAggregate.ChangeMyTitle(command.Title);
        }
        public Task HandleAsync(ICommandContext context, ChangeNothingCommand command)
        {
            return Task.CompletedTask;
        }
        public class SetApplicatonMessageCommandHandler : ICommandHandler<SetApplicatonMessageCommand>
        {
            public Task HandleAsync(ICommandContext context, SetApplicatonMessageCommand command)
            {
                context.SetApplicationMessage(new TestApplicationMessage(command.AggregateRootId));
                return Task.CompletedTask;
            }
        }
        public Task HandleAsync(ICommandContext context, SetResultCommand command)
        {
            context.Add(new TestAggregate(command.AggregateRootId, ""));
            context.SetResult(command.Result);
            return Task.CompletedTask;
        }
        public async Task HandleAsync(ICommandContext context, ChangeMultipleAggregatesCommand command)
        {
            var testAggregate1 = await context.GetAsync<TestAggregate>(command.AggregateRootId1);
            var testAggregate2 = await context.GetAsync<TestAggregate>(command.AggregateRootId2);
            testAggregate1.TestEvents();
            testAggregate2.TestEvents();
        }
        public Task HandleAsync(ICommandContext context, ThrowExceptionCommand command)
        {
            throw new Exception("CommandException");
        }
        public async Task HandleAsync(ICommandContext context, AggregateThrowExceptionCommand command)
        {
            var testAggregate = await context.GetAsync<TestAggregate>(command.AggregateRootId);
            testAggregate.ThrowException(command.IsDomainException);
        }
        public async Task HandleAsync(ICommandContext context, TestEventPriorityCommand command)
        {
            var testAggregate = await context.GetAsync<TestAggregate>(command.AggregateRootId);
            testAggregate.TestEvents();
        }
    }

    public class TestCommandHandler1 : ICommandHandler<TwoHandlersCommand>
    {
        public Task HandleAsync(ICommandContext context, TwoHandlersCommand command)
        {
            return Task.CompletedTask;
        }
    }
    public class TestCommandHandler2 : ICommandHandler<TwoHandlersCommand>
    {
        public Task HandleAsync(ICommandContext context, TwoHandlersCommand command)
        {
            return Task.CompletedTask;
        }
    }
    public class BaseCommandHandler : ICommandHandler<BaseCommand>
    {
        public Task HandleAsync(ICommandContext context, BaseCommand command)
        {
            context.SetResult("ResultFromBaseCommand");
            return Task.CompletedTask;
        }
    }
    public class ChildCommandHandler : ICommandHandler<ChildCommand>
    {
        public Task HandleAsync(ICommandContext context, ChildCommand command)
        {
            context.SetResult("ResultFromChildCommand");
            return Task.CompletedTask;
        }
    }

    public class TestApplicationMessage : ApplicationMessage
    {
        public string AggregateRootId { get; set; }

        public TestApplicationMessage() { }
        public TestApplicationMessage(string aggregateRootId)
        {
            AggregateRootId = aggregateRootId;
        }
    }
}
