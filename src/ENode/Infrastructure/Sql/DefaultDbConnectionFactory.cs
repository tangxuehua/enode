using System.Data;
using System.Data.SqlClient;

namespace ENode.Infrastructure.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultDbConnectionFactory : IDbConnectionFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}