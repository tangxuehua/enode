using ENode.Configurations;

namespace ENode.CommandProcessorPerfTests
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
