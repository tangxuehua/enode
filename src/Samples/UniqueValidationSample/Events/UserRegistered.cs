using System;
using ENode.Eventing;

namespace UniqueValidationSample.Events
{
    [Serializable]
    public class UserRegistered : Event
    {
        public Guid UserId { get; private set; }
        public string UserName { get; private set; }

        public UserRegistered(Guid userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }
    }
}
