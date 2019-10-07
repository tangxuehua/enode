using System.Threading.Tasks;
using ECommon.IO;
using ENode.Messaging;
using ENode.Tests.Domain;

namespace ENode.Tests.EventHandlers
{
    public class TestAggregateEventHandler : IMessageHandler<TestAggregateCreated>
    {
        public Task<AsyncTaskResult> HandleAsync(TestAggregateCreated evnt)
        {
            //DO NOTHING
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
}
