using System;
using ENode.Commanding;

namespace UniquenessConstraintSample
{
    [Serializable]
    public class CreateSectionCommand : Command<string>, ICreatingAggregateCommand
    {
        public string Name { get; private set; }

        public CreateSectionCommand(string name)
        {
            Name = name;
        }
    }
}
