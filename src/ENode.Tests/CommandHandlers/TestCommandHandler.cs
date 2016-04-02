using System;
using System.Threading.Tasks;
using ECommon.IO;
using ENode.Commanding;
using ENode.Infrastructure;
using ENode.Tests.Commands;
using ENode.Tests.Domain;

namespace ENode.Tests.CommandHandlers
{
    public class TestCommandHandler :
        ICommandHandler<TestEventPriorityCommand>,
        ICommandHandler<ChangeMultipleAggregatesCommand>,
        ICommandHandler<ChangeNothingCommand>,
        ICommandHandler<ThrowExceptionCommand>
    {
        public void Handle(ICommandContext context, ChangeNothingCommand command)
        {
            //DO NOTHING
        }
        public void Handle(ICommandContext context, ChangeMultipleAggregatesCommand command)
        {
            context.Get<TestAggregate>(command.AggregateRootId1).TestEvents();
            context.Get<TestAggregate>(command.AggregateRootId2).TestEvents();
        }
        public void Handle(ICommandContext context, ThrowExceptionCommand command)
        {
            throw new Exception("CommandException");
        }
        public void Handle(ICommandContext context, TestEventPriorityCommand command)
        {
            context.Get<TestAggregate>(command.AggregateRootId).TestEvents();
        }
    }

    public class TestCommandHandler1 : ICommandHandler<TwoHandlersCommand>
    {
        public void Handle(ICommandContext context, TwoHandlersCommand command)
        {
            //DO NOTHING
        }
    }
    public class TestCommandHandler2 : ICommandHandler<TwoHandlersCommand>
    {
        public void Handle(ICommandContext context, TwoHandlersCommand command)
        {
            //DO NOTHING
        }
    }
    public class BaseCommandHandler : ICommandHandler<BaseCommand>
    {
        public void Handle(ICommandContext context, BaseCommand command)
        {
            context.SetResult("ResultFromBaseCommand");
        }
    }
    public class ChildCommandHandler : ICommandHandler<ChildCommand>
    {
        public void Handle(ICommandContext context, ChildCommand command)
        {
            context.SetResult("ResultFromChildCommand");
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
