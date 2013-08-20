using System;
using System.Collections.Generic;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultEventTableNameProvider : IEventTableNameProvider
    {
        private readonly string _tableName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultEventTableNameProvider(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }
            _tableName = tableName;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public string GetTable(string aggregateRootId, Type aggregateRootType)
        {
            return _tableName;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllTables()
        {
            return new string[] { _tableName };
        }
    }
}
