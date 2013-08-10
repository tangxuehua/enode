using System;

namespace ENode.Commanding {
    [Serializable]
    public class CommandTimeoutException : Exception {
        public CommandTimeoutException(Guid commandId, Type commandType)
            : base(string.Format("Handle {0} timeout, command Id:{1}", commandType.Name, commandId)) { }
    }
}
