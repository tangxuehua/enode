using System;
using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    public interface IMailBox<TMessage, TMessageProcessResult>
        where TMessage : class, IMailBoxMessage<TMessage, TMessageProcessResult>
    {
        string RoutingKey { get; }
        DateTime LastActiveTime { get; }
        bool IsRunning { get; }
        bool IsPauseRequested { get; }
        bool IsPaused { get; }
        long ConsumingSequence { get; }
        long ConsumedSequence { get; }
        long MaxMessageSequence { get; }
        long TotalUnConsumedMessageCount { get; }

        void EnqueueMessage(TMessage message);
        void TryRun();
        void Pause();
        void Resume();
        void CompleteRun();
        void ResetConsumingSequence(long consumingSequence);
        void Clear();
        Task CompleteMessage(TMessage message, TMessageProcessResult result);
        bool IsInactive(int timeoutSeconds);
    }
}
