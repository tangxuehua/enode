using System.Collections.Generic;
using ENode.Commanding;
using ENode.Domain;

namespace ENode.Infrastructure.Impl
{
    public class DefaultHandlingContext : IHandlingContext
    {
        private readonly IList<ICommand> _commands = new List<ICommand>();
        private readonly IRepository _repository;

        public DefaultHandlingContext(IRepository repository)
        {
            _repository = repository;
        }

        public T Get<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return _repository.Get<T>(aggregateRootId);
        }
        public void AddCommand(ICommand command)
        {
            _commands.Add(command);
        }
        public IEnumerable<ICommand> GetCommands()
        {
            return _commands;
        }
    }
}
