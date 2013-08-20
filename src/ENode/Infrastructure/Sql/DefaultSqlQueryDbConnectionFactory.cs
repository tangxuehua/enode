using System;
using System.Data;
using System.Data.SqlClient;

namespace ENode.Infrastructure.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultSqlQueryDbConnectionFactory : ISqlQueryDbConnectionFactory
    {
        private readonly string _connectionString;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultSqlQueryDbConnectionFactory(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            _connectionString = connectionString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}