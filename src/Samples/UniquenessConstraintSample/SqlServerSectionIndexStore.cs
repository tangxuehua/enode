using System.Data.SqlClient;
using System.Linq;
using ECommon.Dapper;

namespace UniquenessConstraintSample
{
    public class SqlServerSectionIndexStore : ISectionIndexStore
    {
        private readonly string _connectionString;
        private readonly string _sectionIndexTable;

        public SqlServerSectionIndexStore(string connectionString, string sectionIndexTable)
        {
            _connectionString = connectionString;
            _sectionIndexTable = sectionIndexTable;
        }

        public SectionIndex FindBySectionId(string sectionId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var data = connection.QueryList(new { SectionId = sectionId }, _sectionIndexTable).SingleOrDefault();
                if (data != null)
                {
                    var indexId = data.IndexId as string;
                    var sectionName = data.SectionName as string;
                    return new SectionIndex(indexId, sectionId, sectionName);
                }
                return null;
            }
        }
        public SectionIndex FindBySectionName(string sectionName)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var data = connection.QueryList(new { SectionName = sectionName }, _sectionIndexTable).SingleOrDefault();
                if (data != null)
                {
                    var indexId = data.IndexId as string;
                    var sectionId = data.SectionId as string;
                    return new SectionIndex(indexId, sectionId, sectionName);
                }
                return null;
            }
        }
        public void Add(SectionIndex index)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                connection.Insert(index, _sectionIndexTable);
            }
        }
        public void Update(string indexId, string sectionName)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                connection.Update(new { SectionName = sectionName }, new { IndexId = indexId }, _sectionIndexTable);
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
