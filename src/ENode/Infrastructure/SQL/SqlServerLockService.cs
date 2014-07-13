using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ECommon.Dapper;

namespace ENode.Infrastructure.SQL
{
    public class SqlServerLockService : ILockService
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _lockKeyTable;
        private readonly string _lockKeySqlFormat;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SqlServerLockService(string connectionString, string lockKeyTable)
        {
            if (string.IsNullOrEmpty(lockKeyTable))
            {
                throw new ArgumentNullException("lockKeyTable");
            }

            _connectionString = connectionString;
            _lockKeyTable = lockKeyTable;
            _lockKeySqlFormat = "SELECT * FROM [" + lockKeyTable + "] WITH (UPDLOCK) WHERE [LockKey] = '{0}'";
        }

        #endregion

        public void AddLockKey(string lockKey)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var count = connection.QueryList(new { LockKey = lockKey }, _lockKeyTable).Count();
                if (count == 0)
                {
                    connection.Insert(new { LockKey = lockKey }, _lockKeyTable);
                }
            }
        }
        public void ExecuteInLock(string lockKey, Action action)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    LockKey(transaction, lockKey);
                    action();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void LockKey(IDbTransaction transaction, string key)
        {
            var sql = string.Format(_lockKeySqlFormat, key);
            transaction.Connection.Query(sql, transaction: transaction);
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
