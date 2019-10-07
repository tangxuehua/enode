using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Remoting;
using ECommon.Scheduling;
using ECommon.Serializing;
using ECommon.Socketing;
using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Messaging;
using EQueue.Broker;
using EQueue.Configurations;
using EQueue.NameServer;
using EQueue.Protocols;
using EQueue.Protocols.NameServers;
using EQueue.Protocols.NameServers.Requests;

namespace NoteSample.QuickStart
{
    public static class ENodeExtensions
    {
        private static NameServerController _nameServerController;
        private static BrokerController _broker;
        private static CommandService _commandService;
        private static CommandConsumer _commandConsumer;
        private static DomainEventPublisher _eventPublisher;
        private static DomainEventConsumer _eventConsumer;
        private static SocketRemotingClient _nameServerSocketRemotingClient;

        public static ENodeConfiguration BuildContainer(this ENodeConfiguration enodeConfiguration)
        {
            enodeConfiguration.GetCommonConfiguration().BuildContainer();
            return enodeConfiguration;
        }
        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };
            enodeConfiguration.RegisterTopicProviders(assemblies);

            var configuration = enodeConfiguration.GetCommonConfiguration();
            configuration.RegisterEQueueComponents();

            _commandService = new CommandService();
            _eventPublisher = new DomainEventPublisher();

            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            configuration.SetDefault<IMessagePublisher<DomainEventStreamMessage>, DomainEventPublisher>(_eventPublisher);

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var brokerStorePath = ConfigurationManager.AppSettings["equeue-store-path"];
            if (Directory.Exists(brokerStorePath))
            {
                Directory.Delete(brokerStorePath, true);
            }

            var configuration = enodeConfiguration.GetCommonConfiguration();

            _commandService.InitializeEQueue(new CommandResultProcessor().Initialize(new IPEndPoint(SocketUtils.GetLocalIPV4(), 9000)));
            _eventPublisher.InitializeEQueue();
            _commandConsumer = new CommandConsumer().InitializeEQueue().Subscribe(Constants.CommandTopic);
            _eventConsumer = new DomainEventConsumer().InitializeEQueue().Subscribe(Constants.EventTopic);

            _nameServerController = new NameServerController();
            _broker = BrokerController.Create(new BrokerSetting(chunkFileStoreRootPath: brokerStorePath));
            _nameServerSocketRemotingClient = new SocketRemotingClient("NameServerRemotingClient", new IPEndPoint(SocketUtils.GetLocalIPV4(), 9493));

            _nameServerController.Start();
            _broker.Start();
            _eventConsumer.Start();
            _commandConsumer.Start();
            _eventPublisher.Start();
            _commandService.Start();
            _nameServerSocketRemotingClient.Start();

            //生产环境不需要以下这段代码
            CreateTopic(Constants.CommandTopic);
            CreateTopic(Constants.EventTopic);
            WaitAllProducerTopicQueuesAvailable();
            WaitAllConsumerLoadBalanceComplete();

            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandService.Shutdown();
            _eventPublisher.Shutdown();
            _commandConsumer.Shutdown();
            _eventConsumer.Shutdown();
            _broker.Shutdown();
            _nameServerController.Shutdown();
            return enodeConfiguration;
        }

        private static void WaitAllProducerTopicQueuesAvailable()
        {
            var logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(ENodeExtensions).Name);
            var scheduleService = ObjectContainer.Resolve<IScheduleService>();
            var waitHandle = new ManualResetEvent(false);
            logger.Info("Waiting for all producer topic queues available, please wait for a moment...");
            scheduleService.StartTask("WaitAllProducerTopicQueuesAvailable", () =>
            {
                _commandService.Producer.ClientService.LoadTopicMessageQueuesFromNameServerAsync(Constants.CommandTopic).Wait();
                _eventPublisher.Producer.ClientService.LoadTopicMessageQueuesFromNameServerAsync(Constants.EventTopic).Wait();
                var availableQueues1 = _commandService.Producer.GetAvailableMessageQueues(Constants.CommandTopic);
                var availableQueues2 = _eventPublisher.Producer.GetAvailableMessageQueues(Constants.EventTopic);
                if (availableQueues1 != null
                 && availableQueues2 != null
                 && availableQueues1.Count == 4
                 && availableQueues2.Count == 4)
                {
                    waitHandle.Set();
                }
            }, 1000, 1000);

            waitHandle.WaitOne();
            scheduleService.StopTask("WaitAllProducerTopicQueuesAvailable");
            logger.Info("All producer topic queues are available.");
        }
        private static void WaitAllConsumerLoadBalanceComplete()
        {
            var logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(ENodeExtensions).Name);
            var scheduleService = ObjectContainer.Resolve<IScheduleService>();
            var waitHandle = new ManualResetEvent(false);
            logger.Info("Waiting for all consumer load balance complete, please wait for a moment...");
            scheduleService.StartTask("WaitAllConsumerLoadBalanceComplete", () =>
            {
                var eventConsumerAllocatedQueues = _eventConsumer.Consumer.GetCurrentQueues();
                var commandConsumerAllocatedQueues = _commandConsumer.Consumer.GetCurrentQueues();
                if (eventConsumerAllocatedQueues.Count() == 4 && commandConsumerAllocatedQueues.Count() == 4)
                {
                    waitHandle.Set();
                }
            }, 1000, 1000);

            waitHandle.WaitOne();
            scheduleService.StopTask("WaitAllConsumerLoadBalanceComplete");
            logger.Info("All consumer load balance completed.");
        }
        private static void CreateTopic(string topic)
        {
            var binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            var requestData = binarySerializer.Serialize(new CreateTopicForClusterRequest
            {
                ClusterName = "DefaultCluster",
                Topic = topic
            });
            var remotingRequest = new RemotingRequest((int)NameServerRequestCode.CreateTopic, requestData);
            var task = _nameServerSocketRemotingClient.InvokeAsync(remotingRequest, 30000);
            task.Wait();
            if (task.Result.ResponseCode != ResponseCode.Success)
            {
                throw new Exception(string.Format("CreateTopic failed, errorMessage: {0}", Encoding.UTF8.GetString(task.Result.ResponseBody)));
            }
        }
    }
}
