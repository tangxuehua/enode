namespace ENode.Eventing.Impl.SQL
{
    public class SqlEventStoreSetting
    {
        public string ConnectionString { get; set; }
        public string CommitLogTable { get; set; }
        public string CommandIndexTable { get; set; }
        public string VersionIndexTable { get; set; }
        public string AggregateVersionTable { get; set; }
    }
}
