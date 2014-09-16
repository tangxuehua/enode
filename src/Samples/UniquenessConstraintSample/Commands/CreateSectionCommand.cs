using System;
using ENode.Commanding;

namespace UniquenessConstraintSample
{
    [Serializable]
    public class CreateSectionCommand : AggregateCommand<string>, ICreatingAggregateCommand
    {
        public string Name { get; private set; }

        public CreateSectionCommand(string name)
        {
            Name = name;
        }
    }
}
