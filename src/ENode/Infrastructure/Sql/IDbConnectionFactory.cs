using System.Data;

namespace ENode.Infrastructure.Sql
{
    /// <summary>Represents a factory to create db connection.
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>Create a new db connection with the given connection string.
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection(string connectionString);
    }
}