using System;
using ENode.Domain;
using UniqueValidationSample.Events;

namespace UniqueValidationSample.Domain
{
    [Serializable]
    public class User : AggregateRoot<Guid>
    {
        public string Name { get; private set; }

        public User(string name)
        {
            RaiseEvent(new UserRegistered(Guid.NewGuid(), name));
        }

        private void Handle(UserRegistered evnt)
        {
            Id = evnt.SourceId;
            Name = evnt.UserName;
        }
    }
}
