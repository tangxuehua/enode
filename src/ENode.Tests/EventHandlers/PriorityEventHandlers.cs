using System.Collections.Generic;
using System.Threading.Tasks;
using ECommon.IO;
using ECommon.Logging;
using ENode.Eventing;
using ENode.Messaging;
using ENode.Tests.Domain;

namespace ENode.Tests
{
    [Priority(1)]
    public class Handler1 : IMessageHandler<Event1>
    {
        private ILogger _logger;

        public Handler1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler1).Name);
        }

        [Priority(4)]
        public Task HandleAsync(Event1 evnt)
        {
            _logger.Info("event1 handled by handler1.");
            CommandAndEventServiceTest.HandlerTypes.AddOrUpdate(1,
            x => new List<string> { GetType().Name },
            (x, existing) =>
            {
                existing.Add(GetType().Name);
                return existing;
            });
            return Task.CompletedTask;
        }
    }
    [Priority(3)]
    public class Handler2 : IMessageHandler<Event1>
    {
        private ILogger _logger;

        public Handler2(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler2).Name);
        }

        public Task HandleAsync(Event1 evnt)
        {
            _logger.Info("event1 handled by handler2.");
            CommandAndEventServiceTest.HandlerTypes.AddOrUpdate(1,
            x => new List<string> { GetType().Name },
            (x, existing) =>
            {
                existing.Add(GetType().Name);
                return existing;
            });
            return Task.CompletedTask;
        }
    }
    [Priority(2)]
    public class Handler3 : IMessageHandler<Event1>
    {
        private ILogger _logger;

        public Handler3(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler3).Name);
        }

        public Task HandleAsync(Event1 evnt)
        {
            _logger.Info("event1 handled by handler3.");
            CommandAndEventServiceTest.HandlerTypes.AddOrUpdate(1,
            x => new List<string> { GetType().Name },
            (x, existing) =>
            {
                existing.Add(GetType().Name);
                return existing;
            });
            return Task.CompletedTask;
        }
    }

    [Priority(3)]
    public class Handler121 : IMessageHandler<Event1, Event2>
    {
        private ILogger _logger;

        public Handler121(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler121).Name);
        }

        public Task HandleAsync(Event1 evnt, Event2 evnt2)
        {
            _logger.Info("event1,event2 handled by handler1.");
            CommandAndEventServiceTest.HandlerTypes.AddOrUpdate(2,
            x => new List<string> { GetType().Name },
            (x, existing) =>
            {
                existing.Add(GetType().Name);
                return existing;
            });
            return Task.CompletedTask;
        }
    }
    [Priority(2)]
    public class Handler122 : IMessageHandler<Event1, Event2>
    {
        private ILogger _logger;

        public Handler122(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler122).Name);
        }

        public Task HandleAsync(Event1 evnt, Event2 evnt2)
        {
            _logger.Info("event1,event2 handled by handler2.");
            CommandAndEventServiceTest.HandlerTypes.AddOrUpdate(2,
            x => new List<string> { GetType().Name },
            (x, existing) =>
            {
                existing.Add(GetType().Name);
                return existing;
            });
            return Task.CompletedTask;
        }
    }
    [Priority(1)]
    public class Handler123 : IMessageHandler<Event1, Event2>
    {
        private ILogger _logger;

        public Handler123(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler123).Name);
        }

        [Priority(4)]
        public Task HandleAsync(Event1 evnt, Event2 evnt2)
        {
            _logger.Info("event1,event2 handled by handler3.");
            CommandAndEventServiceTest.HandlerTypes.AddOrUpdate(2,
            x => new List<string> { GetType().Name },
            (x, existing) =>
            {
                existing.Add(GetType().Name);
                return existing;
            });
            return Task.CompletedTask;
        }
    }

    [Priority(3)]
    public class Handler1231 : IMessageHandler<Event1, Event2, Event3>
    {
        private ILogger _logger;

        public Handler1231(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler1231).Name);
        }

        public Task HandleAsync(Event1 evnt, Event2 evnt2, Event3 evnt3)
        {
            _logger.Info("event1,event2,event3 handled by handler1.");
            CommandAndEventServiceTest.HandlerTypes.AddOrUpdate(3,
            x => new List<string> { GetType().Name },
            (x, existing) =>
            {
                existing.Add(GetType().Name);
                return existing;
            });
            return Task.CompletedTask;
        }
    }
    [Priority(2)]
    public class Handler1232 : IMessageHandler<Event1, Event2, Event3>
    {
        private ILogger _logger;

        public Handler1232(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler1232).Name);
        }

        public Task HandleAsync(Event1 evnt, Event2 evnt2, Event3 evnt3)
        {
            _logger.Info("event1,event2,event3 handled by handler2.");
            CommandAndEventServiceTest.HandlerTypes.AddOrUpdate(3,
            x => new List<string> { GetType().Name },
            (x, existing) =>
            {
                existing.Add(GetType().Name);
                return existing;
            });
            return Task.CompletedTask;
        }
    }
    [Priority(1)]
    public class Handler1233 : IMessageHandler<Event1, Event2, Event3>
    {
        private ILogger _logger;

        public Handler1233(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(Handler1233).Name);
        }

        [Priority(4)]
        public Task HandleAsync(Event1 evnt, Event2 evnt2, Event3 evnt3)
        {
            _logger.Info("event1,event2,event3 handled by handler3.");
            CommandAndEventServiceTest.HandlerTypes.AddOrUpdate(3,
            x => new List<string> { GetType().Name },
            (x, existing) =>
            {
                existing.Add(GetType().Name);
                return existing;
            });
            return Task.CompletedTask;
        }
    }
}
