using ENode.Domain;
using System;
using UniqueValidationSample.Events;

namespace UniqueValidationSample.Domain
{
    [Serializable]
    public class User : AggregateRoot<Guid>
    {
        public string Name { get; private set; }

        public User(string name) : base(Guid.NewGuid())
        {
            RaiseEvent(new UserRegistered(Id, name));
        }

        private void Handle(UserRegistered evnt)
        {
            Name = evnt.UserName;
        }
    }
}
