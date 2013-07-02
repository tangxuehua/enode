using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    public class DefaultEventPersistenceSynchronizerProvider : IEventPersistenceSynchronizerProvider
    {
        public DefaultEventPersistenceSynchronizerProvider(params Assembly[] assemblies)
        {
        }

        public IEnumerable<IEventPersistenceSynchronizer> GetSynchronizers(EventStream eventStream)
        {
            return null;
        }
    }
}
