using ECommon.Utilities;

namespace ENode.Configurations
{
    public class DbTableSetting
    {
        private ConfigurationSetting _configurationSetting;
        private string _connectionString;

        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = _configurationSetting.SqlServerDefaultConnectionString;
                }
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }
        public string TableName { get; set; }
        public string PrimaryKeyName { get; set; }
        public string CommandIndexName { get; set; }

        public DbTableSetting(ConfigurationSetting configurationSetting)
        {
            Ensure.NotNull(configurationSetting, "configurationSetting");
            _configurationSetting = configurationSetting;
        }
    }
}
