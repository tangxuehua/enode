using System;

namespace UniquenessConstraintSample
{
    /// <summary>一个服务，用于提供事务支持
    /// </summary>
    public interface ITransactionService
    {
        ITransaction BeginTransaction();
    }
    /// <summary>表示一个事务接口
    /// </summary>
    public interface ITransaction
    {
        void Commit();
        void Rollback();
    }
}
