using ECommon.Socketing;

namespace ENode.Distribute.EventStore
{
    public class EventStoreClientSetting
    {
        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public int CheckAvailableInterval { get; set; }

        public EventStoreClientSetting()
        {
            ServerAddress = SocketUtils.GetLocalIPV4().ToString();
            ServerPort = 10000;
            CheckAvailableInterval = 1000 * 3;
        }
    }
}
