using ENode.Commanding;
using ENode.Infrastructure;
using UniqueValidationSample.Commands;
using UniqueValidationSample.Domain;

namespace UniqueValidationSample.CommandHandlers
{
    [Component]
    public class RegisterUserCommandHandler : ICommandHandler<RegisterUser>
    {
        public void Handle(ICommandContext context, RegisterUser command)
        {
            context.Add(new User(command.UserId, command.UserName));
        }
    }
}
