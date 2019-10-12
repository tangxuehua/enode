using System;
using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    /// <summary>定义一个用于实现锁的接口
    /// </summary>
    public interface ILockService
    {
        /// <summary>Add a lock key.
        /// </summary>
        /// <param name="lockKey"></param>
        Task AddLockKey(string lockKey);
        /// <summary>Execute the given action with the given lock key.
        /// </summary>
        /// <param name="lockKey"></param>
        /// <param name="action"></param>
        Task ExecuteInLock(string lockKey, Func<Task> action);
    }
}
