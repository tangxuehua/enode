using System.Threading;
using ENode.Commanding;
using ENode.EQueue;

namespace DistributeSample.CommandProducer.EQueueIntegrations
{
    public class CommandTopicManager : ICommandTopicProvider
    {
        static int _index;
        public string GetTopic(ICommand command)
        {
            if (Interlocked.Increment(ref _index) % 2 == 0)
            {
                return "NoteCommandTopic2";
            }
            else
            {
                return "NoteCommandTopic1";
            }
        }
    }
}
