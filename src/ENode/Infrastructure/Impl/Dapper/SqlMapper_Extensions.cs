using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ENode;

namespace Dapper {
    /// <summary>Dapper extensions by tangxuehua, 2012-11-21
    /// </summary>
    public partial class SqlMapper {
        private static ConcurrentDictionary<Type, List<string>> _paramNameCache = new ConcurrentDictionary<Type, List<string>>();

        public static long? Insert(this IDbConnection connection, dynamic data, string table, IDbTransaction transaction = null, int? commandTimeout = null) {
            var obj = data as object;
            var properties = GetProperties(obj);
            var columns = string.Join(",", properties);
            var values = string.Join(",", properties.Select(p => "@" + p));
            var sql = string.Format("insert into [{0}] ({1}) values ({2}) select cast(scope_identity() as bigint)", table, columns, values);

            return SqlMapper.Query<long?>(connection, sql, obj, transaction, true, commandTimeout).SingleOrDefault();
        }
        public static int Update(this IDbConnection connection, dynamic data, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null) {
            var obj = data as object;
            var properties = GetProperties(obj);
            var updateFields = string.Join(",", properties.Select(p => p + " = @" + p));

            var conditionObj = condition as object;
            var whereProperties = GetProperties(conditionObj);
            var where = string.Join(" and ", whereProperties.Select(p => p + " = @" + p));

            var sql = string.Format("update [{0}] set {1} where {2}", table, updateFields, where);

            var parameters = new DynamicParameters(data);
            parameters.AddDynamicParams(condition);

            return SqlMapper.Execute(connection, sql, parameters, transaction, commandTimeout);
        }
        public static int Delete(this IDbConnection connection, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null) {
            var properties = GetProperties(condition as object);
            var whereFields = string.Join(" and ", properties.Select(p => p + " = @" + p));
            var sql = string.Format("delete from [{0}] where {1}", table, whereFields);

            return SqlMapper.Execute(connection, sql, condition, transaction, commandTimeout);
        }
        public static int GetCount(this IDbConnection connection, dynamic condition, string table, bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null) {
            var obj = condition as object;
            var properties = GetProperties(obj);
            var whereFields = string.Empty;
            if (isOr) {
                whereFields = string.Join(" or ", properties.Select(p => p + " = @" + p));
            }
            else {
                whereFields = string.Join(" and ", properties.Select(p => p + " = @" + p));
            }
            var sql = string.Format("select count(*) from [{0}] where {1}", table, whereFields);

            return SqlMapper.Query<int>(connection, sql, obj, transaction, true, commandTimeout).Single();
        }
        public static T GetValue<T>(this IDbConnection connection, dynamic condition, string table, string field, IDbTransaction transaction = null, int? commandTimeout = null) {
            var obj = condition as object;
            var properties = GetProperties(obj);
            var whereFields = string.Join(" and ", properties.Select(p => p + " = @" + p));
            var sql = string.Format("select {2} from [{0}] where {1}", table, whereFields, field);

            return SqlMapper.Query<T>(connection, sql, obj, transaction, true, commandTimeout).SingleOrDefault();
        }
        public static IEnumerable<dynamic> QueryAll(this IDbConnection connection, string table, IDbTransaction transaction = null, int? commandTimeout = null) {
            var sql = string.Format("select * from [{0}]", table);
            return SqlMapper.Query(connection, sql, null, transaction, true, commandTimeout);
        }
        public static IEnumerable<T> QueryAll<T>(this IDbConnection connection, string table, IDbTransaction transaction = null, int? commandTimeout = null) {
            var sql = string.Format("select * from [{0}]", table);
            return SqlMapper.Query<T>(connection, sql, null, transaction, true, commandTimeout);
        }

        public static IEnumerable<dynamic> Query(this IDbConnection connection, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null) {
            var obj = condition as object;
            var properties = GetProperties(obj);
            var whereFields = string.Join(" and ", properties.Select(p => p + " = @" + p));
            var sql = string.Format("select * from [{0}] where {1}", table, whereFields);

            return SqlMapper.Query(connection, sql, obj, transaction, true, commandTimeout);
        }
        public static IEnumerable<T> Query<T>(this IDbConnection connection, object condition, string table, IDbTransaction transaction = null, int? commandTimeout = null) {
            var obj = condition as object;
            var properties = GetProperties(obj);
            var whereFields = string.Join(" and ", properties.Select(p => p + " = @" + p));
            var sql = string.Format("select * from [{0}] where {1}", table, whereFields);

            return SqlMapper.Query<T>(connection, sql, obj, transaction, true, commandTimeout);
        }

        public static void TryExecute(this IDbConnection connection, Action<IDbConnection> action) {
            using (connection) {
                connection.Open();
                action(connection);
            }
        }
        public static T TryExecute<T>(this IDbConnection connection, Func<IDbConnection, T> func) {
            using (connection) {
                connection.Open();
                return func(connection);
            }
        }
        public static void TryExecuteInTransaction(this IDbConnection connection, Action<IDbConnection, IDbTransaction> action) {
            using (connection) {
                IDbTransaction transaction = null;
                try {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    action(connection, transaction);
                    transaction.Commit();
                }
                catch {
                    if (transaction != null) {
                        transaction.Rollback();
                    }
                    throw;
                }
            }
        }
        public static T TryExecuteInTransaction<T>(this IDbConnection connection, Func<IDbConnection, IDbTransaction, T> func) {
            using (connection) {
                IDbTransaction transaction = null;
                T result;
                try {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    result = func(connection, transaction);
                    transaction.Commit();
                    return result;
                }
                catch {
                    if (transaction != null) {
                        transaction.Rollback();
                    }
                    throw;
                }
            }
        }

        private static List<string> GetProperties(object o) {
            if (o is DynamicParameters) {
                return (o as DynamicParameters).ParameterNames.ToList();
            }

            List<string> properties;
            if (!_paramNameCache.TryGetValue(o.GetType(), out properties)) {
                properties = new List<string>();
                foreach (var prop in o.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)) {
                    properties.Add(prop.Name);
                }
                _paramNameCache[o.GetType()] = properties;
            }
            return properties;
        }
    }
}
