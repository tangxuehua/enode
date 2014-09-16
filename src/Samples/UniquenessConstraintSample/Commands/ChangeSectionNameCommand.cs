using System;
using ENode.Commanding;

namespace UniquenessConstraintSample
{
    [Serializable]
    public class ChangeSectionNameCommand : AggregateCommand<string>
    {
        public string Name { get; private set; }

        public ChangeSectionNameCommand(string id, string name) : base(id)
        {
            Name = name;
        }
    }
}
