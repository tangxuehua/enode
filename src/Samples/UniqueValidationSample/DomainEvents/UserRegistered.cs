using System;
using ENode.Eventing;

namespace UniqueValidationSample.DomainEvents
{
    [Serializable]
    public class UserRegistered : DomainEvent<Guid>, ISourcingEvent
    {
        public string UserName { get; private set; }

        public UserRegistered(Guid userId, string userName) : base(userId)
        {
            UserName = userName;
        }
    }
}
