using System.Collections.Generic;
using ENode.Commanding;
using ENode.Domain;
using ENode.Eventing;
using ENode.Exceptions;

namespace ENode.Infrastructure
{
    public class MessageHandlerContext : IEventContext, IExceptionHandlingContext
    {
        private readonly List<ICommand> _commands = new List<ICommand>();
        private readonly IRepository _repository;

        public MessageHandlerContext(IRepository repository)
        {
            _repository = repository;
        }

        public virtual T Get<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return _repository.Get<T>(aggregateRootId);
        }
        public virtual void AddCommand(ICommand command)
        {
            _commands.Add(command);
        }
        public virtual IEnumerable<ICommand> GetCommands()
        {
            return _commands;
        }
    }
}
