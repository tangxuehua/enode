using System;
using ENode.Domain;
using UniqueValidationSample.DomainEvents;

namespace UniqueValidationSample.Domain
{
    [Serializable]
    public class User : AggregateRoot<Guid>
    {
        public string Name { get; private set; }

        public User(Guid userId, string name)
        {
            RaiseEvent(new UserRegistered(userId, name));
        }

        private void Handle(UserRegistered evnt)
        {
            Id = evnt.SourceId;
            Name = evnt.UserName;
        }
    }
}
