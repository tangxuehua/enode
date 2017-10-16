using ENode.Configurations;

namespace ENode.EventStorePerfTests
{
    public static class ENodeExtensions
    {
        public static ENodeConfiguration BuildContainer(this ENodeConfiguration enodeConfiguration)
        {
            enodeConfiguration.GetCommonConfiguration().BuildContainer();
            return enodeConfiguration;
        }
    }
}
