using System;
using System.IO;
using ENode.Infrastructure;
using log4net;
using log4net.Config;

namespace ENode.Infrastructure
{
    public class Log4NetLoggerFactory : ILoggerFactory
    {
        public Log4NetLoggerFactory(string configFile)
        {
            var file = new FileInfo(configFile);
            if (!file.Exists)
            {
                file = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile));
            }
            if (!file.Exists)
            {
                throw new FileNotFoundException("Log4Net config file not exist.", configFile);
            }
            XmlConfigurator.ConfigureAndWatch(file);
        }

        public ILogger Create(string name)
        {
            return new Log4NetLogger(LogManager.GetLogger(name));
        }
        public ILogger Create(Type type)
        {
            return new Log4NetLogger(LogManager.GetLogger(type));
        }
    }
}
