using System;
using System.Reflection;
using ECommon.Autofac;
using ECommon.Configurations;
using ECommon.IoC;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.ProtocolBuf;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.EQueue;
using ENode.Eventing;
using EQueue.Clients.Producers;
using EQueue.Protocols;
using EQueue.Utils;
using NoteSample.Commands;
using NoteSample.Domain;
using NoteSample.DomainEvents;
using NoteSample.EQueueIntegrations;
using ProtoBuf.Meta;

namespace NoteSample
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();

            var noteId = Guid.NewGuid();
            var command1 = new CreateNoteCommand(noteId, "Note Version1");
            var command2 = new ChangeNoteTitleCommand(noteId, "Note Version2");

            Console.WriteLine(string.Empty);

            commandService.Execute(command1).Wait();
            commandService.Execute(command2).Wait();

            Console.WriteLine(string.Empty);

            Console.WriteLine("Press Enter to exit...");

            Console.ReadLine();
        }

        static void InitializeENodeFramework()
        {
            ConfigProtobufMetaData();

            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .UseProtoBufSerializer()
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .UseEQueue()
                .InitializeENode(assemblies)
                .StartEQueue()
                .StartEnode();
        }
        static void ConfigProtobufMetaData()
        {
            var model = RuntimeTypeModel.Default;

            //Config equeue classes.
            model.Add(typeof(TypeData<string>), false).Add("TypeCode", "Data").UseConstructor = false;
            model.Add(typeof(TypeData<byte[]>), false).Add("TypeCode").UseConstructor = false;
            model[typeof(TypeData<string>)].AddSubType(10, typeof(StringTypeData)).UseConstructor = false;
            model[typeof(TypeData<byte[]>)].AddSubType(10, typeof(ByteTypeData)).UseConstructor = false;

            model.Add(typeof(ConsumerData), false).Add("ConsumerId", "GroupName", "SubscriptionTopics").UseConstructor = false;
            model.Add(typeof(Message), false).Add("Topic").UseConstructor = false;
            model[typeof(Message)].AddSubType(10, typeof(QueueMessage)).UseConstructor = false;
            model.Add(typeof(QueueMessage), false).Add("MessageOffset", "QueueId", "QueueOffset", "StoredTime").UseConstructor = false;
            model.Add(typeof(MessageQueue), false).Add("Topic", "QueueId").UseConstructor = false;
            model.Add(typeof(PullMessageRequest), false).Add("ConsumerGroup", "MessageQueue", "QueueOffset", "PullMessageBatchSize").UseConstructor = false;
            model.Add(typeof(PullMessageResponse), false).Add("Messages").UseConstructor = false;
            model.Add(typeof(QueryConsumerRequest), false).Add("GroupName", "Topic").UseConstructor = false;
            model.Add(typeof(SendMessageRequest), false).Add("QueueId", "Message").UseConstructor = false;
            model.Add(typeof(SendMessageResponse), false).Add("MessageOffset", "MessageQueue", "QueueOffset").UseConstructor = false;
            model.Add(typeof(SendResult), false).Add("SendStatus", "ErrorMessage", "MessageQueue", "QueueOffset", "MessageOffset").UseConstructor = false;

            model.Add(typeof(CommandMessage), false).Add("CommandData", "CommandExecutedMessageTopic", "DomainEventHandledMessageTopic").UseConstructor = false;
            model.Add(typeof(CommandExecutedMessage), false).Add("CommandId", "AggregateRootId", "ProcessId", "CommandStatus", "ExceptionCode", "ErrorMessage").UseConstructor = false;
            model.Add(typeof(DomainEventHandledMessage), false).Add("CommandId", "AggregateRootId", "IsProcessCompletedEvent", "ProcessId").UseConstructor = false;
            model.Add(typeof(EventMessage), false).Add("CommandId", "AggregateRootId", "AggregateRootName", "Version", "Timestamp", "Events", "ContextItems").UseConstructor = false;

            //Config enode base classes.
            model.Add(typeof(AggregateRoot<Guid>), false).Add("_id", "_uniqueId", "_version").UseConstructor = false;

            model.Add(typeof(Command<Guid>), false).Add("Id", "RetryCount", "AggregateRootId").UseConstructor = false;
            model.Add(typeof(ProcessCommand<Guid>), false).Add("_processId").UseConstructor = false;

            model.Add(typeof(DomainEvent<Guid>), false).Add("Id", "AggregateRootId").UseConstructor = false;

            model[typeof(Command<Guid>)].AddSubType(10, typeof(ProcessCommand<Guid>)).UseConstructor = false;

            //Config sample project classes.
            model.Add(typeof(Note), false).Add("Title", "CreatedTime", "UpdatedTime").UseConstructor = false;

            model.Add(typeof(CreateNoteCommand), false).Add("Title").UseConstructor = false;
            model.Add(typeof(ChangeNoteTitleCommand), false).Add("Title").UseConstructor = false;

            model.Add(typeof(NoteCreatedEvent), false).Add("Title", "CreatedTime", "UpdatedTime").UseConstructor = false;
            model.Add(typeof(NoteTitleChangedEvent), false).Add("Title", "UpdatedTime").UseConstructor = false;

            model[typeof(AggregateRoot<Guid>)].AddSubType(100, typeof(Note)).UseConstructor = false;

            model[typeof(Command<Guid>)].AddSubType(100, typeof(CreateNoteCommand)).UseConstructor = false;
            model[typeof(Command<Guid>)].AddSubType(101, typeof(ChangeNoteTitleCommand)).UseConstructor = false;

            model[typeof(DomainEvent<Guid>)].AddSubType(100, typeof(NoteCreatedEvent)).UseConstructor = false;
            model[typeof(DomainEvent<Guid>)].AddSubType(101, typeof(NoteTitleChangedEvent)).UseConstructor = false;
        }
    }
}
