using System.Data;

namespace ENode.Infrastructure.Sql
{
    /// <summary>Represents a factory to create sql query db connection.
    /// </summary>
    public interface ISqlQueryDbConnectionFactory
    {
        /// <summary>Create a new sql query db connection.
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();
    }
}