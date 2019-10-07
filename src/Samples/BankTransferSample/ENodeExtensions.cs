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
using ENode.Domain;
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

namespace BankTransferSample
{
    public static class ENodeExtensions
    {
        private static NameServerController _nameServerController;
        private static BrokerController _broker;
        private static CommandService _commandService;
        private static CommandConsumer _commandConsumer;
        private static ApplicationMessagePublisher _applicationMessagePublisher;
        private static ApplicationMessageConsumer _applicationMessageConsumer;
        private static DomainEventPublisher _domainEventPublisher;
        private static DomainEventConsumer _eventConsumer;
        private static DomainExceptionPublisher _exceptionPublisher;
        private static DomainExceptionConsumer _exceptionConsumer;
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
            _applicationMessagePublisher = new ApplicationMessagePublisher();
            _domainEventPublisher = new DomainEventPublisher();
            _exceptionPublisher = new DomainExceptionPublisher();

            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            configuration.SetDefault<IMessagePublisher<IApplicationMessage>, ApplicationMessagePublisher>(_applicationMessagePublisher);
            configuration.SetDefault<IMessagePublisher<DomainEventStreamMessage>, DomainEventPublisher>(_domainEventPublisher);
            configuration.SetDefault<IMessagePublisher<IDomainException>, DomainExceptionPublisher>(_exceptionPublisher);

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var brokerStorePath = ConfigurationManager.AppSettings["equeue-store-path"];
            if (Directory.Exists(brokerStorePath))
            {
                Directory.Delete(brokerStorePath, true);
            }

            _commandService.InitializeEQueue(new CommandResultProcessor().Initialize(new IPEndPoint(SocketUtils.GetLocalIPV4(), 9000)));
            _applicationMessagePublisher.InitializeEQueue();
            _domainEventPublisher.InitializeEQueue();
            _exceptionPublisher.InitializeEQueue();

            _nameServerController = new NameServerController();
            _broker = BrokerController.Create(new BrokerSetting(chunkFileStoreRootPath: brokerStorePath));

            _commandConsumer = new CommandConsumer().InitializeEQueue().Subscribe(Constants.CommandTopic);
            _applicationMessageConsumer = new ApplicationMessageConsumer().InitializeEQueue().Subscribe(Constants.ApplicationMessageTopic);
            _eventConsumer = new DomainEventConsumer().InitializeEQueue().Subscribe(Constants.EventTopic);
            _exceptionConsumer = new DomainExceptionConsumer().InitializeEQueue().Subscribe(Constants.ExceptionTopic);
            _nameServerSocketRemotingClient = new SocketRemotingClient("NameServerRemotingClient", new IPEndPoint(SocketUtils.GetLocalIPV4(), 9493));

            _nameServerController.Start();
            _broker.Start();
            _exceptionConsumer.Start();
            _eventConsumer.Start();
            _applicationMessageConsumer.Start();
            _commandConsumer.Start();
            _applicationMessagePublisher.Start();
            _domainEventPublisher.Start();
            _exceptionPublisher.Start();
            _commandService.Start();
            _nameServerSocketRemotingClient.Start();

            //生产环境不需要以下这段代码
            CreateTopic(Constants.CommandTopic);
            CreateTopic(Constants.EventTopic);
            CreateTopic(Constants.ApplicationMessageTopic);
            CreateTopic(Constants.ExceptionTopic);
            WaitAllProducerTopicQueuesAvailable();
            WaitAllConsumerLoadBalanceComplete();

            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandService.Shutdown();
            _applicationMessagePublisher.Shutdown();
            _domainEventPublisher.Shutdown();
            _exceptionPublisher.Shutdown();
            _commandConsumer.Shutdown();
            _applicationMessageConsumer.Shutdown();
            _eventConsumer.Shutdown();
            _exceptionConsumer.Shutdown();
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
                _domainEventPublisher.Producer.ClientService.LoadTopicMessageQueuesFromNameServerAsync(Constants.EventTopic).Wait();
                _applicationMessagePublisher.Producer.ClientService.LoadTopicMessageQueuesFromNameServerAsync(Constants.ApplicationMessageTopic).Wait();
                _exceptionPublisher.Producer.ClientService.LoadTopicMessageQueuesFromNameServerAsync(Constants.ExceptionTopic).Wait();
                var availableQueues1 = _commandService.Producer.GetAvailableMessageQueues(Constants.CommandTopic);
                var availableQueues2 = _domainEventPublisher.Producer.GetAvailableMessageQueues(Constants.EventTopic);
                var availableQueues3 = _applicationMessagePublisher.Producer.GetAvailableMessageQueues(Constants.ApplicationMessageTopic);
                var availableQueues4 = _exceptionPublisher.Producer.GetAvailableMessageQueues(Constants.ExceptionTopic);
                if (availableQueues1 != null
                 && availableQueues1 != null
                 && availableQueues1 != null
                 && availableQueues1 != null
                 && availableQueues1.Count == 4
                 && availableQueues2.Count == 4
                 && availableQueues3.Count == 4
                 && availableQueues4.Count == 4)
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
                var commandConsumerAllocatedQueues = _commandConsumer.Consumer.GetCurrentQueues();
                var eventConsumerAllocatedQueues = _eventConsumer.Consumer.GetCurrentQueues();
                var exceptionConsumerAllocatedQueues = _exceptionConsumer.Consumer.GetCurrentQueues();
                if (commandConsumerAllocatedQueues.Count() == 4 && eventConsumerAllocatedQueues.Count() == 4 && exceptionConsumerAllocatedQueues.Count() == 4)
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
