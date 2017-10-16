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

        private string _connectionString;
        private string _tableName;
        private string _lockKeySqlFormat;

        #endregion

        public SqlServerLockService Initialize(OptionSetting optionSetting)
        {
            if (optionSetting != null)
            {
                _connectionString = optionSetting.GetOptionValue<string>("ConnectionString");
                _tableName = optionSetting.GetOptionValue<string>("TableName");
            }
            else
            {
                var setting = ENodeConfiguration.Instance.Setting.DefaultDBConfigurationSetting;
                _connectionString = setting.ConnectionString;
                _tableName = setting.LockKeyTableName;
            }

            Ensure.NotNull(_connectionString, "_connectionString");
            Ensure.NotNull(_tableName, "_tableName");

            _lockKeySqlFormat = "SELECT * FROM [" + _tableName + "] WITH (UPDLOCK) WHERE [Name] = '{0}'";

            return this;
        }
        public void AddLockKey(string lockKey)
        {
            using (var connection = GetConnection())
            {
                var count = connection.QueryList(new { Name = lockKey }, _tableName).Count();
                if (count == 0)
                {
                    connection.Insert(new { Name = lockKey }, _tableName);
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
