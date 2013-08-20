using System;
using System.Collections;

namespace ENode.Infrastructure.Concurrent
{
    /// <summary>提供根据值对象锁行为的实用方法
    /// </summary>
    public static class LockUtility
    {
        private class LockObject
        {
            public int Counter { get; set; }
        }

        /// <summary>锁对象池, 所有引用计数大于0的锁对象都会在池中缓存起来
        /// </summary>
        private static readonly Hashtable LockPool = new Hashtable();

        /// <summary>该方法可以根据某个值对象的值来锁住响应的行为。
        /// 用法类似系统lock方法，功能是锁住某个指定的key，只要key的值相同，那么action的执行就不允许并发。
        /// <remarks>
        /// 设计该方法是为了弥补.net框架的lock方法的局限性。.net框架的lock方法只能锁引用相同的对象。
        /// 但是很多时候我们希望锁住某个值对象，只要该值对象的值相同，则action就不允许并发。
        /// </remarks>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public static void Lock(object key, Action action)
        {
            var lockObj = GetLockObject(key);
            try
            {
                lock (lockObj)
                {
                    action();
                }
            }
            finally
            {
                ReleaseLockObject(key, lockObj);
            }
        }

        /// <summary>释放锁对象, 当锁的引用计数为0时, 从锁对象池移除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="lockObj"></param>
        private static void ReleaseLockObject(object key, LockObject lockObj)
        {
            lockObj.Counter--;
            lock (LockPool)
            {
                if (lockObj.Counter == 0)
                {
                    LockPool.Remove(key);
                }
            }
        }
        /// <summary>从锁对象池中获取锁对象, 并且锁对象的引用计数加1.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static LockObject GetLockObject(object key)
        {
            lock (LockPool)
            {
                var lockObj = LockPool[key] as LockObject;
                if (lockObj == null)
                {
                    lockObj = new LockObject();
                    LockPool[key] = lockObj;
                }
                lockObj.Counter++;
                return lockObj;
            }
        }
    }
}