using System;
using ENode.Eventing;

namespace UniqueValidationSample.Events
{
    [Serializable]
    public class UserRegistered : Event
    {
        public string UserName { get; private set; }

        public UserRegistered(Guid userId, string userName) : base(userId)
        {
            UserName = userName;
        }
    }
}
