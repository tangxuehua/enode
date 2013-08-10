using System;
using ENode.Domain;
using ENode.Eventing;
using UniqueValidationSample.Events;

namespace UniqueValidationSample.Domain {
    [Serializable]
    public class User : AggregateRoot<Guid>, IEventHandler<UserRegistered> {
        public string Name { get; private set; }

        public User() : base() { }
        public User(string name)
            : base(Guid.NewGuid()) {
            RaiseEvent(new UserRegistered(Id, name));
        }

        void IEventHandler<UserRegistered>.Handle(UserRegistered evnt) {
            Name = evnt.UserName;
        }
    }
}
