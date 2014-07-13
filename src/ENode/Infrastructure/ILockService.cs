using System;

namespace ENode.Infrastructure
{
    /// <summary>定义一个用于实现锁的接口
    /// </summary>
    public interface ILockService
    {
        void AddLockKey(string lockKey);
        void ExecuteInLock(string lockKey, Action action);
    }
}
