using System;
using ENode.Eventing;

namespace UniquenessConstraintSample
{
    [Serializable]
    public class SectionNameChangedEvent : DomainEvent<string>
    {
        public string Name { get; private set; }

        public SectionNameChangedEvent(string id, string name) : base(id)
        {
            Name = name;
        }
    }
}
