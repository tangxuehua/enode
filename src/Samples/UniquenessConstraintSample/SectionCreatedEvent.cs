using System;
using ENode.Eventing;

namespace UniquenessConstraintSample
{
    [Serializable]
    public class SectionCreatedEvent : DomainEvent<string>
    {
        public string Name { get; private set; }

        public SectionCreatedEvent(string id, string name) : base(id)
        {
            Name = name;
        }
    }
}
