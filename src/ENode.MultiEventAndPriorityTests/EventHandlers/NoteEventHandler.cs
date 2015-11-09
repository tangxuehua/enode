using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Logging;
using ENode.Infrastructure;
using NoteSample.Domain;

namespace ENode.MultiEventAndPriorityTests.EventHandlers
{
    [Component]
    [Priority(1)]
    [Code(1)]
    public class Handler1 : IMessageHandler<Event1>
    {
        private ILogger _logger;

        public Handler1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler1).Name);
        }

        [Priority(4)]
        public Task<AsyncTaskResult> HandleAsync(Event1 evnt)
        {
            _logger.Info("event1 handled by handler1.");
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
    [Component]
    [Priority(3)]
    [Code(2)]
    public class Handler2 : IMessageHandler<Event1>
    {
        private ILogger _logger;

        public Handler2(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler2).Name);
        }

        public Task<AsyncTaskResult> HandleAsync(Event1 evnt)
        {
            _logger.Info("event1 handled by handler2.");
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
    [Component]
    [Priority(2)]
    [Code(3)]
    public class Handler3 : IMessageHandler<Event1>
    {
        private ILogger _logger;

        public Handler3(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler3).Name);
        }

        public Task<AsyncTaskResult> HandleAsync(Event1 evnt)
        {
            _logger.Info("event1 handled by handler3.");
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }

    [Component]
    [Priority(3)]
    [Code(4)]
    public class Handler121 : IMessageHandler<Event1, Event2>
    {
        private ILogger _logger;

        public Handler121(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler121).Name);
        }

        public Task<AsyncTaskResult> HandleAsync(Event1 evnt, Event2 evnt2)
        {
            _logger.Info("event1,event2 handled by handler1.");
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
    [Component]
    [Priority(2)]
    [Code(5)]
    public class Handler122 : IMessageHandler<Event1, Event2>
    {
        private ILogger _logger;

        public Handler122(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler122).Name);
        }

        public Task<AsyncTaskResult> HandleAsync(Event1 evnt, Event2 evnt2)
        {
            _logger.Info("event1,event2 handled by handler2.");
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
    [Component]
    [Priority(1)]
    [Code(6)]
    public class Handler123 : IMessageHandler<Event1, Event2>
    {
        private ILogger _logger;

        public Handler123(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler123).Name);
        }

        [Priority(4)]
        public Task<AsyncTaskResult> HandleAsync(Event1 evnt, Event2 evnt2)
        {
            _logger.Info("event1,event2 handled by handler3.");
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }

    [Component]
    [Priority(3)]
    [Code(7)]
    public class Handler1231 : IMessageHandler<Event1, Event2, Event3>
    {
        private ILogger _logger;

        public Handler1231(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler1231).Name);
        }

        public Task<AsyncTaskResult> HandleAsync(Event1 evnt, Event2 evnt2, Event3 evnt3)
        {
            _logger.Info("event1,event2,event3 handled by handler1.");
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
    [Component]
    [Priority(2)]
    [Code(8)]
    public class Handler1232 : IMessageHandler<Event1, Event2, Event3>
    {
        private ILogger _logger;

        public Handler1232(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler1232).Name);
        }

        public Task<AsyncTaskResult> HandleAsync(Event1 evnt, Event2 evnt2, Event3 evnt3)
        {
            _logger.Info("event1,event2,event3 handled by handler2.");
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
    [Component]
    [Priority(1)]
    [Code(9)]
    public class Handler1233 : IMessageHandler<Event1, Event2, Event3>
    {
        private ILogger _logger;

        public Handler1233(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler1233).Name);
        }

        [Priority(4)]
        public Task<AsyncTaskResult> HandleAsync(Event1 evnt, Event2 evnt2, Event3 evnt3)
        {
            _logger.Info("event1,event2,event3 handled by handler3.");
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
}
