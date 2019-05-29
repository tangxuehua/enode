using System;
using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    public interface IAggregateMessageMailBox<TMessage, TMessageProcessResult>
        where TMessage : class, IAggregateMessageMailBoxMessage<TMessage, TMessageProcessResult>
    {
        string AggregateRootId { get; }
        DateTime LastActiveTime { get; }
        bool IsRunning { get; }
        long ConsumingSequence { get; }
        long ConsumedSequence { get; }
        long MaxMessageSequence { get; }
        long TotalUnConsumedMessageCount { get; }

        void EnqueueMessage(TMessage message);
        void TryRun(bool exitFirst = false);
        Task Run();
        void Pause();
        void Resume();
        void ResetConsumingSequence(long consumingSequence);
        void Exit();
        void Clear();
        Task CompleteMessage(TMessage message, TMessageProcessResult result);
        bool IsInactive(int timeoutSeconds);
    }
}
