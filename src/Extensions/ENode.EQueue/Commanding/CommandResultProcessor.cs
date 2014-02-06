using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ECommon.IoC;
using ECommon.Scheduling;
using ECommon.Serializing;
using ECommon.Socketing;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;

namespace ENode.EQueue.Commanding
{
    public class CommandResultProcessor : IMessageHandler
    {
        private readonly Consumer _consumer;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<CommandResult>> _processingCommandDict;
        private readonly ConcurrentDictionary<object, TaskCompletionSource<CommandResult>> _processingProcessDict;
        private readonly BlockingCollection<CommandResult> _queue;
        private readonly Worker _worker;
        private readonly static ConsumerSetting _consumerSetting = new ConsumerSetting
        {
            MessageModel = MessageModel.BroadCasting
        };
        public string CommandResultTopic { get; private set; }

        public Consumer Consumer { get { return _consumer; } }

        public CommandResultProcessor()
            : this(_consumerSetting)
        {
        }
        public CommandResultProcessor(ConsumerSetting setting)
            : this(setting, null)
        {
        }
        public CommandResultProcessor(ConsumerSetting setting, string groupName)
            : this(setting, null, groupName)
        {
        }
        public CommandResultProcessor(ConsumerSetting setting, string name, string groupName)
            : this(string.Format("{0}@{1}@{2}", SocketUtils.GetLocalIPV4(), string.IsNullOrEmpty(name) ? typeof(CommandResultProcessor).Name : name, ObjectId.GenerateNewId()), setting, groupName)
        {
        }
        public CommandResultProcessor(string id, ConsumerSetting setting, string groupName)
        {
            _consumer = new Consumer(id, setting, string.IsNullOrEmpty(groupName) ? typeof(CommandResultProcessor).Name + "Group_" + ObjectId.GenerateNewId().ToString() : groupName, this);
            _processingCommandDict = new ConcurrentDictionary<Guid, TaskCompletionSource<CommandResult>>();
            _processingProcessDict = new ConcurrentDictionary<object, TaskCompletionSource<CommandResult>>();
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _queue = new BlockingCollection<CommandResult>(new ConcurrentQueue<CommandResult>());
            _worker = new Worker(() =>
            {
                var commandResult = _queue.Take();
                var taskCompletionSource = default(TaskCompletionSource<CommandResult>);

                if (commandResult.IsProcessCompletedEventPublished)
                {
                    if (_processingProcessDict.TryGetValue(commandResult.AggregateRootId, out taskCompletionSource))
                    {
                        taskCompletionSource.TrySetResult(commandResult);
                    }
                }
                else
                {
                    if (_processingCommandDict.TryGetValue(commandResult.CommandId, out taskCompletionSource))
                    {
                        taskCompletionSource.TrySetResult(commandResult);
                    }
                }
            });
        }

        public CommandResultProcessor RegisterProcessingCommand(ICommand command, TaskCompletionSource<CommandResult> taskCompletionSource)
        {
            if (command is IStartProcessCommand)
            {
                _processingProcessDict.TryAdd(((IStartProcessCommand)command).ProcessId, taskCompletionSource);
            }
            else
            {
                _processingCommandDict.TryAdd(command.Id, taskCompletionSource);
            }
            return this;
        }
        public CommandResultProcessor NotifyCommandSendFailed(ICommand command)
        {
            var taskCompletionSource = default(TaskCompletionSource<CommandResult>);
            if (_processingCommandDict.TryGetValue(command.Id, out taskCompletionSource))
            {
                taskCompletionSource.TrySetResult(new CommandResult(command, "Command send failed."));
            }
            return this;
        }
        public CommandResultProcessor Start()
        {
            _consumer.Start();
            _worker.Start();
            return this;
        }
        public CommandResultProcessor Subscribe(string topic)
        {
            if (CommandResultTopic != null)
            {
                throw new ENodeException("Command result processor can only subscribe one topic.");
            }
            _consumer.Subscribe(topic);
            CommandResultTopic = topic;
            return this;
        }
        public CommandResultProcessor Shutdown()
        {
            _consumer.Shutdown();
            _worker.Stop();
            return this;
        }

        void IMessageHandler.Handle(QueueMessage message, IMessageContext context)
        {
            var commandResult = _binarySerializer.Deserialize(message.Body, typeof(CommandResult)) as CommandResult;
            _queue.Add(commandResult);
        }
    }
}
