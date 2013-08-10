using System;
using System.Collections.Concurrent;

namespace ENode.Commanding {
    public class DefaultProcessingCommandCache : IProcessingCommandCache {
        private ConcurrentDictionary<Guid, CommandInfo> _commandInfoDict = new ConcurrentDictionary<Guid, CommandInfo>();

        public void Add(ICommand command) {
            _commandInfoDict.TryAdd(command.Id, new CommandInfo(command));
        }
        public void TryRemove(Guid commandId) {
            CommandInfo commandInfo;
            _commandInfoDict.TryRemove(commandId, out commandInfo);
        }
        public CommandInfo Get(Guid commandId) {
            CommandInfo commandInfo;
            if (_commandInfoDict.TryGetValue(commandId, out commandInfo)) {
                return commandInfo;
            }
            return null;
        }
    }
}
