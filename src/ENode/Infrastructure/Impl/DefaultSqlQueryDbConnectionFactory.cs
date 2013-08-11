using System;
using System.Data;
using System.Data.SqlClient;
using ENode.Infrastructure;

namespace ENode.Messaging {
    public class DefaultSqlQueryDbConnectionFactory : ISqlQueryDbConnectionFactory {
        private string _connectionString;

        public DefaultSqlQueryDbConnectionFactory(string connectionString) {
            if (connectionString == null) {
                throw new ArgumentNullException("connectionString");
            }
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection() {
            return new SqlConnection(_connectionString);
        }
    }
}