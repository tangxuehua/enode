using System;

namespace ENode.Commanding {
    [Serializable]
    public class AggregateRootNotFoundException : Exception {
        private const string ExceptionMessage = "Cannot find the aggregate {0} of id {1}.";

        public AggregateRootNotFoundException(string id, Type type) : base(string.Format(ExceptionMessage, type.Name, id)) { }
    }
}
