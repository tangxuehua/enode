using System;
using ENode.Domain;

namespace UniquenessConstraintSample
{
    /// <summary>论坛版块，聚合根，版块名称不能重复，名称可以修改
    /// </summary>
    [Serializable]
    public class Section : AggregateRoot<string>
    {
        public string Name { get; private set; }

        public Section(string id, string name) : base(id)
        {
            ApplyEvent(new SectionCreatedEvent(Id, name));
        }

        public void ChangeName(string name)
        {
            ApplyEvent(new SectionNameChangedEvent(Id, name));
        }

        private void Handle(SectionCreatedEvent evnt)
        {
            Id = evnt.AggregateRootId;
            Name = evnt.Name;
        }
        private void Handle(SectionNameChangedEvent evnt)
        {
            Name = evnt.Name;
        }
    }
}
