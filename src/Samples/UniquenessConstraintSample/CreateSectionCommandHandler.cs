using System;
using ECommon.Components;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Infrastructure;

namespace UniquenessConstraintSample
{
    [Component]
    public class CreateSectionCommandHandler : ICommandHandler<CreateSectionCommand>
    {
        private ILockService _lockService;
        private ISectionIndexStore _indexStore;

        public CreateSectionCommandHandler(ILockService lockService, ISectionIndexStore indexStore)
        {
            _lockService = lockService;
            _indexStore = indexStore;
        }

        public void Handle(ICommandContext context, CreateSectionCommand command)
        {
            _lockService.ExecuteInLock(typeof(Section).Name, () =>
            {
                var sectionIndex = _indexStore.FindBySectionName(command.Name);
                if (sectionIndex == null)
                {
                    var sectionId = ObjectId.GenerateNewStringId();
                    context.Add(new Section(sectionId, command.Name));
                    _indexStore.Add(new SectionIndex(command.Id, sectionId, command.Name));
                }
                else if (sectionIndex.IndexId == command.Id)
                {
                    context.Add(new Section(sectionIndex.SectionId, command.Name));
                }
                else
                {
                    throw new Exception("Duplicate section name:" + command.Name);
                }
            });
        }
    }
}
