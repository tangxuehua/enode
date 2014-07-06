using System;

namespace UniquenessConstraintSample
{
    /// <summary>定义一个用于实现锁的接口
    /// </summary>
    public interface ILockService
    {
        void Lock(string key);
    }
}
