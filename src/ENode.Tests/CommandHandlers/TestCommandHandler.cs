using System;
using System.Threading;
using System.Threading.Tasks;
using ECommon.IO;
using ENode.Commanding;
using ENode.Infrastructure;
using ENode.Tests.Commands;
using ENode.Tests.Domain;

namespace ENode.Tests.CommandHandlers
{
    public class TestCommandHandler :
        ICommandHandler<CreateTestAggregateCommand>,
        ICommandHandler<ChangeTestAggregateTitleCommand>,
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
        public Task HandleAsync(ICommandContext context, ChangeNothingCommand command)
        {
            return Task.CompletedTask;
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
            testAggregate.ThrowException(command.PublishableException);
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

    public class AsyncHandlerCommandHandler : ICommandAsyncHandler<AsyncHandlerCommand>
    {
        private int _count;

        public bool CheckCommandHandledFirst
        {
            get { return true; }
        }

        public Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(AsyncHandlerCommand command)
        {
            if (command.ShouldGenerateApplicationMessage)
            {
                return Task.FromResult(new AsyncTaskResult<IApplicationMessage>(AsyncTaskStatus.Success, new TestApplicationMessage(command.AggregateRootId)));
            }
            else if (command.ShouldThrowException)
            {
                throw new Exception("AsyncCommandException");
            }
            else if (command.ShouldThrowIOException)
            {
                _count++;
                if (_count <= 5)
                {
                    throw new IOException("AsyncCommandIOException" + _count);
                }
                _count = 0;
                return Task.FromResult(new AsyncTaskResult<IApplicationMessage>(AsyncTaskStatus.Success));
            }
            else
            {
                return Task.FromResult(new AsyncTaskResult<IApplicationMessage>(AsyncTaskStatus.Success));
            }
        }
    }
    public class TestCommandAsyncHandler1 : ICommandAsyncHandler<TwoAsyncHandlersCommand>
    {
        public bool CheckCommandHandledFirst
        {
            get { return true; }
        }
        public Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(TwoAsyncHandlersCommand command)
        {
            return Task.FromResult(new AsyncTaskResult<IApplicationMessage>(AsyncTaskStatus.Success));
        }
    }
    public class TestCommandAsyncHandler2 : ICommandAsyncHandler<TwoAsyncHandlersCommand>
    {
        public bool CheckCommandHandledFirst
        {
            get { return true; }
        }
        public Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(TwoAsyncHandlersCommand command)
        {
            return Task.FromResult(new AsyncTaskResult<IApplicationMessage>(AsyncTaskStatus.Success));
        }
    }
    public class NotCheckAsyncHandlerExistCommandHandler : ICommandAsyncHandler<NotCheckAsyncHandlerExistCommand>
    {
        public bool CheckCommandHandledFirst
        {
            get { return false; }
        }
        public Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(NotCheckAsyncHandlerExistCommand command)
        {
            return Task.FromResult(new AsyncTaskResult<IApplicationMessage>(AsyncTaskStatus.Success));
        }
    }
    public class NotCheckAsyncHandlerExistWithResultCommandHandler : ICommandAsyncHandler<NotCheckAsyncHandlerExistWithResultCommand>
    {
        public bool CheckCommandHandledFirst
        {
            get { return false; }
        }
        public Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(NotCheckAsyncHandlerExistWithResultCommand command)
        {
            return Task.FromResult(new AsyncTaskResult<IApplicationMessage>(AsyncTaskStatus.Success, new TestApplicationMessage(command.AggregateRootId)));
        }
    }
    public class AsyncHandlerBaseCommandAsyncHandler : ICommandAsyncHandler<AsyncHandlerBaseCommand>
    {
        public bool CheckCommandHandledFirst
        {
            get { return true; }
        }
        public Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(AsyncHandlerBaseCommand command)
        {
            return Task.FromResult(new AsyncTaskResult<IApplicationMessage>(AsyncTaskStatus.Success));
        }
    }
    public class AsyncHandlerChildCommandAsyncHandler : ICommandAsyncHandler<AsyncHandlerChildCommand>
    {
        public bool CheckCommandHandledFirst
        {
            get { return true; }
        }
        public Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(AsyncHandlerChildCommand command)
        {
            return Task.FromResult(new AsyncTaskResult<IApplicationMessage>(AsyncTaskStatus.Success));
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

        public override string GetRoutingKey()
        {
            return AggregateRootId;
        }
    }
}
