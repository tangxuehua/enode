using System.Collections.Generic;
using ECommon.Utilities;

namespace ENode.Configurations
{
    public class OptionSetting
    {
        private IDictionary<string, object> _options;

        public OptionSetting(params KeyValuePair<string, object>[] options)
        {
            _options = new Dictionary<string, object>();
            if (options == null || options.Length == 0)
            {
                return;
            }
            foreach (var option in options)
            {
                _options.Add(option.Key, option.Value);
            }
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
