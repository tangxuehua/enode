using System;
using System.Threading;
using ECommon.Components;
using ENode.Commanding;
using ENode.Infrastructure;

namespace UniquenessConstraintSample
{
    [Component]
    public class ChangeSectionNameCommandHandler : ICommandHandler<ChangeSectionNameCommand>
    {
        private ILockService _lockService;
        private ISectionIndexStore _indexStore;
        private static int _duplicateCount;

        public ChangeSectionNameCommandHandler(ILockService lockService, ISectionIndexStore indexStore)
        {
            _lockService = lockService;
            _indexStore = indexStore;
        }

        public void Handle(ICommandContext context, ChangeSectionNameCommand command)
        {
            _lockService.ExecuteInLock(typeof(Section).Name, () =>
            {
                var existingIndex = _indexStore.FindBySectionId(command.AggregateRootId);
                if (existingIndex == null)
                {
                    throw new Exception("Section index not exist, sectionId:" + command.AggregateRootId);
                }

                var sectionIndex = _indexStore.FindBySectionName(command.Name);
                if (sectionIndex == null)
                {
                    context.Get<Section>(command.AggregateRootId).ChangeName(command.Name);
                    _indexStore.Update(existingIndex.IndexId, command.Name);
                }
                else if (sectionIndex.IndexId == existingIndex.IndexId)
                {
                    context.Get<Section>(command.AggregateRootId).ChangeName(command.Name);
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
