using System;
using ENode.Commanding;

namespace UniqueValidationSample.Commands
{
    [Serializable]
    public class RegisterUser : Command, ICreatingAggregateCommand
    {
        public Guid UserId { get; private set; }
        public string UserName { get; private set; }

        public RegisterUser(Guid userId, string userName) : base(userId)
        {
            UserId = userId;
            UserName = userName;
        }
    }
}
