using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    public class DefaultEventTableNameProvider : IEventTableNameProvider
    {
        private string _tableName;

        public DefaultEventTableNameProvider(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }
            _tableName = tableName;
        }

        public string GetTable(string aggregateRootId, Type aggregateRootType)
        {
            return _tableName;
        }
        public IEnumerable<string> GetAllTables()
        {
            return new string[] { _tableName };
        }
    }
}
