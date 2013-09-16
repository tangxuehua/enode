using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ENode.Infrastructure.Dapper
{
    /// <summary>Dapper extensions by tangxuehua, 2012-11-21
    /// </summary>
    public partial class SqlMapper
    {
        private static readonly ConcurrentDictionary<Type, List<string>> ParamNameCache = new ConcurrentDictionary<Type, List<string>>();

        /// <summary>Insert data into table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Insert(this IDbConnection connection, dynamic data, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = data as object;
            var properties = GetProperties(obj);
            var columns = string.Join(",", properties);
            var values = string.Join(",", properties.Select(p => "@" + p));
            var sql = string.Format("insert into [{0}] ({1}) values ({2}) select cast(scope_identity() as bigint)", table, columns, values);

            return connection.Execute(sql, obj, transaction, commandTimeout);
        }
        /// <summary>
        /// 
        /// </summary>Updata data for table with a specified condition.
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update(this IDbConnection connection, dynamic data, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = data as object;
            var properties = GetProperties(obj);
            var updateFields = string.Join(",", properties.Select(p => p + " = @" + p));

            var conditionObj = condition as object;
            var whereProperties = GetProperties(conditionObj);
            var where = string.Join(" and ", whereProperties.Select(p => p + " = @" + p));

            var sql = string.Format("update [{0}] set {1} where {2}", table, updateFields, where);

            var parameters = new DynamicParameters(data);
            parameters.AddDynamicParams(condition);

            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }
        /// <summary>Delete data from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Delete(this IDbConnection connection, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var properties = GetProperties(condition as object);
            var whereFields = string.Join(" and ", properties.Select(p => p + " = @" + p));
            var sql = string.Format("delete from [{0}] where {1}", table, whereFields);

            return SqlMapper.Execute(connection, sql, condition, transaction, commandTimeout);
        }
        /// <summary>Get data count from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int GetCount(this IDbConnection connection, dynamic condition, string table, bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = condition as object;
            var properties = GetProperties(obj);
            var whereFields = isOr ? string.Join(" or ", properties.Select(p => p + " = @" + p)) : string.Join(" and ", properties.Select(p => p + " = @" + p));
            var sql = string.Format("select count(*) from [{0}] where {1}", table, whereFields);

            return connection.Query<int>(sql, obj, transaction, true, commandTimeout).Single();
        }
        /// <summary>Get a field value from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="field"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetValue<T>(this IDbConnection connection, dynamic condition, string table, string field, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = condition as object;
            var properties = GetProperties(obj);
            var whereFields = string.Join(" and ", properties.Select(p => p + " = @" + p));
            var sql = string.Format("select {2} from [{0}] where {1}", table, whereFields, field);

            return connection.Query<T>(sql, obj, transaction, true, commandTimeout).SingleOrDefault();
        }
        /// <summary>Query all data from table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> QueryAll(this IDbConnection connection, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var sql = string.Format("select * from [{0}]", table);
            return connection.Query(sql, null, transaction, true, commandTimeout);
        }
        /// <summary>Query all data from table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> QueryAll<T>(this IDbConnection connection, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var sql = string.Format("select * from [{0}]", table);
            return connection.Query<T>(sql, null, transaction, true, commandTimeout);
        }

        /// <summary>Query a list of data from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> Query(this IDbConnection connection, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.Query(GetSimpleQuerySQL(condition as object, table), condition as object, transaction, true, commandTimeout);
        }
        /// <summary>Query a single data from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static dynamic QuerySingleOrDefault(this IDbConnection connection, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.Query(GetSimpleQuerySQL(condition as object, table), condition as object, transaction, true, commandTimeout).SingleOrDefault();
        }
        /// <summary>Query a list of data from table with specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this IDbConnection connection, object condition, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.Query<T>(GetSimpleQuerySQL(condition as object, table), condition as object, transaction, true, commandTimeout);
        }
        /// <summary>Query a single data from table with specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T QuerySingleOrDefault<T>(this IDbConnection connection, object condition, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.Query<T>(GetSimpleQuerySQL(condition as object, table), condition as object, transaction, true, commandTimeout).SingleOrDefault();
        }

        /// <summary>Query paged data from a single table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> QueryPaged(this IDbConnection connection, dynamic condition, string table, string columns, string orderBy, int pageIndex, int pageSize, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = condition as object;
            var whereFields = string.Empty;
            var properties = GetProperties(obj);
            if (properties.Count > 0)
            {
                whereFields = " WHERE " + string.Join(" and ", properties.Select(p => p + " = @" + p));
            }
            var sql = string.Format("SELECT {0} FROM (SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS RowNumber, {0} FROM {2} {3}) AS Total WHERE RowNumber BETWEEN {4} AND {5}", columns, orderBy, table, whereFields, pageIndex * pageSize, (pageIndex + 1) * pageSize);

            return connection.Query(sql, obj, transaction, true, commandTimeout);
        }
        /// <summary>Query paged data from a single table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> QueryPaged<T>(this IDbConnection connection, dynamic condition, string table, string columns, string orderBy, int pageIndex, int pageSize, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = condition as object;
            var whereFields = string.Empty;
            var properties = GetProperties(obj);
            if (properties.Count > 0)
            {
                whereFields = " WHERE " + string.Join(" and ", properties.Select(p => p + " = @" + p));
            }
            var sql = string.Format("SELECT {0} FROM (SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS RowNumber, {0} FROM {2}{3}) AS Total WHERE RowNumber BETWEEN {4} AND {5}", columns, orderBy, table, whereFields, pageIndex * pageSize, (pageIndex + 1) * pageSize);

            return connection.Query<T>(sql, obj, transaction, true, commandTimeout);
        }

        /// <summary>Try to execute a given action and auto close the dbconnection after the action complete.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="action"></param>
        public static void TryExecute(this IDbConnection connection, Action<IDbConnection> action)
        {
            using (connection)
            {
                connection.Open();
                action(connection);
            }
        }
        /// <summary>Try to execute a given func and auto close the dbconnection after the action complete.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T TryExecute<T>(this IDbConnection connection, Func<IDbConnection, T> func)
        {
            using (connection)
            {
                connection.Open();
                return func(connection);
            }
        }
        /// <summary>Try to execute a given action in transaction and auto close the dbconnection after the action complete.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="action"></param>
        public static void TryExecuteInTransaction(this IDbConnection connection, Action<IDbConnection, IDbTransaction> action)
        {
            using (connection)
            {
                IDbTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    action(connection, transaction);
                    transaction.Commit();
                }
                catch
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }
                    throw;
                }
            }
        }
        /// <summary>Try to execute a given func in transaction and auto close the dbconnection after the action complete.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T TryExecuteInTransaction<T>(this IDbConnection connection, Func<IDbConnection, IDbTransaction, T> func)
        {
            using (connection)
            {
                IDbTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    var result = func(connection, transaction);
                    transaction.Commit();
                    return result;
                }
                catch
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }
                    throw;
                }
            }
        }

        private static string GetSimpleQuerySQL(dynamic condition, string table)
        {
            var obj = condition as object;
            var properties = GetProperties(obj);
            var whereFields = string.Join(" and ", properties.Select(p => p + " = @" + p));
            var sql = string.Format("select * from [{0}] where {1}", table, whereFields);
            return sql;
        }
        private static List<string> GetProperties(object o)
        {
            if (o == null)
            {
                return new List<string>();
            }
            if (o is DynamicParameters)
            {
                return (o as DynamicParameters).ParameterNames.ToList();
            }

            List<string> properties;
            if (ParamNameCache.TryGetValue(o.GetType(), out properties)) return properties;
            properties = o.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public).Select(prop => prop.Name).ToList();
            ParamNameCache[o.GetType()] = properties;
            return properties;
        }
    }
}
