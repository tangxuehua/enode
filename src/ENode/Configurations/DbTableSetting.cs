using System.Collections.Generic;
using ECommon.Utilities;

namespace ENode.Configurations
{
    public class DbTableSetting
    {
        private ConfigurationSetting _configurationSetting;
        private string _connectionString;
        private IDictionary<string, object> _options;

        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = _configurationSetting.SqlDefaultConnectionString;
                }
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        public DbTableSetting(ConfigurationSetting configurationSetting)
        {
            Ensure.NotNull(configurationSetting, "configurationSetting");
            _configurationSetting = configurationSetting;
            _options = new Dictionary<string, object>();
        }

        public void SetOptionValue(string key, object value)
        {
            _options[key] = value;
        }
        public T GetOptionValue<T>(string key)
        {
            object value;
            if (_options.TryGetValue(key, out value))
            {
                return TypeUtils.ConvertType<T>(value);
            }
            return default(T);
        }
    }
}
