using System.Threading.Tasks;
using ENode.Messaging;
using ENode.Tests.Domain;

namespace ENode.Tests.EventHandlers
{
    public class TestAggregateEventHandler : IMessageHandler<TestAggregateCreated>
    {
        public Task HandleAsync(TestAggregateCreated evnt)
        {
            //DO NOTHING
            return Task.CompletedTask;
        }
    }
}
