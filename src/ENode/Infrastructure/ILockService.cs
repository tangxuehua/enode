using System;

namespace ENode.Infrastructure
{
    /// <summary>定义一个用于实现锁的接口
    /// </summary>
    public interface ILockService
    {
        /// <summary>Add a lock key.
        /// </summary>
        /// <param name="lockKey"></param>
        void AddLockKey(string lockKey);
        /// <summary>Execute the given action with the given lock key.
        /// </summary>
        /// <param name="lockKey"></param>
        /// <param name="action"></param>
        void ExecuteInLock(string lockKey, Action action);
    }
}
