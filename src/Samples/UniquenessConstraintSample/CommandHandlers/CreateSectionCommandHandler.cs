using System;
using System.Threading;
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
        private static int _duplicateCount;

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
                    //这里表示SectionName有重复，这里仅仅简单递增一下计数器，实际应该抛出异常告知Command发起者（controller）
                    Console.WriteLine("duplicateCount:" + Interlocked.Increment(ref _duplicateCount));
                    //throw new Exception("Duplicate section name:" + command.Name);
                }
            });
        }
    }
}
