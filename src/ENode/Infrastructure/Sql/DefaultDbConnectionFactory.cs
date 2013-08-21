using System.Data;
using System.Data.SqlClient;

namespace ENode.Infrastructure.Sql
{
    /// <summary>The default implementation of IDbConnectionFactory.
    /// </summary>
    public class DefaultDbConnectionFactory : IDbConnectionFactory
    {
        /// <summary>Create a db connection instance with the given connectionString.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}