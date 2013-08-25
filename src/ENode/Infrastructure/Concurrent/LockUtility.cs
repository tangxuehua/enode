using System;
using System.Collections;

namespace ENode.Infrastructure.Concurrent
{
    /// <summary>A class provide the functionality to lock object by value object.
    /// </summary>
    public static class LockUtility
    {
        private class LockObject
        {
            public int Counter { get; set; }
        }

        private static readonly Hashtable LockPool = new Hashtable();

        /// <summary>Lock an action by a given key value object.
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