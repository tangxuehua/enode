using ECommon.Components;
using ENode.Configurations;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;
using NoteSample.Domain;

namespace ENode.CommandProcessorPerfTests
{
    [Component]
    public static class ENodeExtensions
    {
        public static ENodeConfiguration RegisterAllTypeCodes(this ENodeConfiguration enodeConfiguration)
        {
            var provider = ObjectContainer.Resolve<ITypeCodeProvider>() as DefaultTypeCodeProvider;

            //aggregates
            provider.RegisterType<Note>(100);

            //events
            provider.RegisterType<NoteCreated>(1000);
            provider.RegisterType<NoteTitleChanged>(1001);

            return enodeConfiguration;
        }
    }
}
