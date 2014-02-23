using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ECommon.IoC;
using ECommon.Scheduling;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;

namespace ENode.EQueue.Commanding
{
    public class CommandResultProcessor
    {
        private readonly Consumer _failedCommandMessageConsumer;
        private readonly Consumer _domainEventHandledMessageConsumer;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<CommandResult>> _commandTaskDict;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<ProcessResult>> _processTaskDict;
        private readonly BlockingCollection<FailedCommandMessage> _failedCommandMessageLocalQueue;
        private readonly BlockingCollection<DomainEventHandledMessage> _domainEventHandledMessageLocalQueue;
        private readonly Worker _failedCommandMessageWorker;
        private readonly Worker _domainEventHandledMessageWorker;
        private readonly IBinarySerializer _binarySerializer;

        public string FailedCommandMessageTopic { get; private set; }
        public string DomainEventHandledMessageTopic { get; private set; }
        public Consumer FailedCommandMessageConsumer { get { return _failedCommandMessageConsumer; } }
        public Consumer DomainEventHandledMessageConsumer { get { return _domainEventHandledMessageConsumer; } }

        public CommandResultProcessor(Consumer failedCommandMessageConsumer, Consumer domainEventHandledMessageConsumer)
        {
            _failedCommandMessageConsumer = failedCommandMessageConsumer;
            _domainEventHandledMessageConsumer = domainEventHandledMessageConsumer;
            _commandTaskDict = new ConcurrentDictionary<Guid, TaskCompletionSource<CommandResult>>();
            _processTaskDict = new ConcurrentDictionary<string, TaskCompletionSource<ProcessResult>>();
            _failedCommandMessageLocalQueue = new BlockingCollection<FailedCommandMessage>(new ConcurrentQueue<FailedCommandMessage>());
            _domainEventHandledMessageLocalQueue = new BlockingCollection<DomainEventHandledMessage>(new ConcurrentQueue<DomainEventHandledMessage>());
            _failedCommandMessageWorker = new Worker(() => ProcessFailedCommandMessage(_failedCommandMessageLocalQueue.Take()));
            _domainEventHandledMessageWorker = new Worker(() => ProcessDomainEventHandledMessage(_domainEventHandledMessageLocalQueue.Take()));
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        public CommandResultProcessor SetFailedCommandMessageTopic(string topic)
        {
            if (FailedCommandMessageTopic != null)
            {
                throw new ENodeException("Failed command message topic can't be set twice.");
            }
            _failedCommandMessageConsumer.Subscribe(topic);
            FailedCommandMessageTopic = topic;
            return this;
        }
        public CommandResultProcessor SetDomainEventHandledMessageTopic(string topic)
        {
            if (DomainEventHandledMessageTopic != null)
            {
                throw new ENodeException("Domain event handled message topic can't be set twice.");
            }
            _domainEventHandledMessageConsumer.Subscribe(topic);
            DomainEventHandledMessageTopic = topic;
            return this;
        }

        public CommandResultProcessor RegisterCommand(ICommand command, TaskCompletionSource<CommandResult> taskCompletionSource)
        {
            _commandTaskDict.TryAdd(command.Id, taskCompletionSource);
            return this;
        }
        public CommandResultProcessor RegisterProcess(IProcessCommand command, TaskCompletionSource<ProcessResult> taskCompletionSource)
        {
            _processTaskDict.TryAdd(command.ProcessId, taskCompletionSource);
            return this;
        }

        public CommandResultProcessor NotifyCommandSendFailed(ICommand command)
        {
            TaskCompletionSource<CommandResult> taskCompletionSource;
            if (_commandTaskDict.TryGetValue(command.Id, out taskCompletionSource))
            {
                taskCompletionSource.TrySetResult(new CommandResult(command.Id, command.AggregateRootId, 0, "Command send failed."));
            }
            return this;
        }
        public CommandResultProcessor NotifyProcessCommandSendFailed(IProcessCommand command)
        {
            TaskCompletionSource<ProcessResult> taskCompletionSource;
            if (_processTaskDict.TryGetValue(command.ProcessId, out taskCompletionSource))
            {
                taskCompletionSource.TrySetResult(new ProcessResult(command.ProcessId, 0, "Start process command send failed."));
            }
            return this;
        }

        public CommandResultProcessor Start()
        {
            _failedCommandMessageConsumer.Start(new FailedCommandMessageHandler(this));
            _domainEventHandledMessageConsumer.Start(new DomainEventHandledMessageHandler(this));
            _failedCommandMessageWorker.Start();
            _domainEventHandledMessageWorker.Start();
            return this;
        }
        public CommandResultProcessor Shutdown()
        {
            _failedCommandMessageConsumer.Shutdown();
            _domainEventHandledMessageConsumer.Shutdown();
            _failedCommandMessageWorker.Stop();
            _domainEventHandledMessageWorker.Stop();
            return this;
        }

        private void ProcessFailedCommandMessage(FailedCommandMessage message)
        {
            TaskCompletionSource<CommandResult> taskCompletionSource;
            if (_commandTaskDict.TryGetValue(message.CommandId, out taskCompletionSource))
            {
                taskCompletionSource.TrySetResult(new CommandResult(message.CommandId, message.AggregateRootId, message.ExceptionCode, message.ErrorMessage));
            }
            if (!string.IsNullOrEmpty(message.ProcessId))
            {
                TaskCompletionSource<ProcessResult> processTaskCompletionSource;
                if (_processTaskDict.TryGetValue(message.ProcessId, out processTaskCompletionSource))
                {
                    processTaskCompletionSource.TrySetResult(new ProcessResult(message.ProcessId, message.ExceptionCode, message.ErrorMessage));
                }
            }
        }
        private void ProcessDomainEventHandledMessage(DomainEventHandledMessage message)
        {
            TaskCompletionSource<CommandResult> taskCompletionSource;
            if (_commandTaskDict.TryGetValue(message.CommandId, out taskCompletionSource))
            {
                taskCompletionSource.TrySetResult(new CommandResult(message.CommandId, message.AggregateRootId));
            }
            if (message.IsProcessCompletedEvent && !string.IsNullOrEmpty(message.ProcessId))
            {
                TaskCompletionSource<ProcessResult> processTaskCompletionSource;
                if (_processTaskDict.TryGetValue(message.ProcessId, out processTaskCompletionSource))
                {
                    processTaskCompletionSource.TrySetResult(new ProcessResult(message.ProcessId));
                }
            }
        }

        class FailedCommandMessageHandler : IMessageHandler
        {
            private CommandResultProcessor _processor;

            public FailedCommandMessageHandler(CommandResultProcessor processor)
            {
                _processor = processor;
            }

            void IMessageHandler.Handle(QueueMessage message, IMessageContext context)
            {
                _processor._failedCommandMessageLocalQueue.Add(_processor._binarySerializer.Deserialize(message.Body, typeof(FailedCommandMessage)) as FailedCommandMessage);
                context.OnMessageHandled(message);
            }
        }
        class DomainEventHandledMessageHandler : IMessageHandler
        {
            private CommandResultProcessor _processor;

            public DomainEventHandledMessageHandler(CommandResultProcessor processor)
            {
                _processor = processor;
            }

            void IMessageHandler.Handle(QueueMessage message, IMessageContext context)
            {
                _processor._domainEventHandledMessageLocalQueue.Add(_processor._binarySerializer.Deserialize(message.Body, typeof(DomainEventHandledMessage)) as DomainEventHandledMessage);
                context.OnMessageHandled(message);
            }
        }
    }
}
