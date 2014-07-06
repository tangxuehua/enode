using System;
using ECommon.Components;
using ENode.Commanding;

namespace UniquenessConstraintSample
{
    [Component]
    public class ChangeSectionNameCommandHandler : ICommandHandler<ChangeSectionNameCommand>
    {
        private ITransactionService _transactionService;
        private ILockService _lockService;
        private ISectionIndexStore _indexStore;

        public ChangeSectionNameCommandHandler(ITransactionService transactionService, ILockService lockService, ISectionIndexStore indexStore)
        {
            _transactionService = transactionService;
            _lockService = lockService;
            _indexStore = indexStore;
        }

        public void Handle(ICommandContext context, ChangeSectionNameCommand command)
        {
            var transaction = _transactionService.BeginTransaction();

            try
            {
                _lockService.Lock("Section");

                var existingIndex = _indexStore.FindBySectionId(command.AggregateRootId);
                if (existingIndex == null)
                {
                    throw new Exception("Section index not exist, sectionId:" + command.AggregateRootId);
                }

                var sectionIndex = _indexStore.FindBySectionName(command.Name);
                if (sectionIndex == null)
                {
                    context.Get<Section>(command.AggregateRootId).ChangeName(command.Name);
                    _indexStore.Update(existingIndex.ChangeSectionName(command.Name));
                }
                else if (sectionIndex.IndexId == existingIndex.IndexId)
                {
                    context.Get<Section>(command.AggregateRootId).ChangeName(command.Name);
                }
                else
                {
                    throw new Exception("Duplicate section name:" + command.Name);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
