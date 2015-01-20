using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using ECommon.Dapper;
using ECommon.Utilities;
using ENode.Configurations;

namespace ENode.Infrastructure.Impl.SQL
{
    public class SqlServerLockService : ILockService
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _lockKeyTable;
        private readonly string _lockKeySqlFormat;

        #endregion

        #region Constructors

        public SqlServerLockService()
        {
            var setting = ENodeConfiguration.Instance.Setting.SqlServerLockServiceSetting;
            Ensure.NotNull(setting, "SqlServerLockServiceSetting");
            Ensure.NotNull(setting.ConnectionString, "SqlServerLockServiceSetting.ConnectionString");
            Ensure.NotNull(setting.TableName, "SqlServerLockServiceSetting.TableName");

            _connectionString = setting.ConnectionString;
            _lockKeyTable = setting.TableName;
            _lockKeySqlFormat = "SELECT * FROM [" + setting.TableName + "] WITH (UPDLOCK) WHERE [LockKey] = '{0}'";
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
