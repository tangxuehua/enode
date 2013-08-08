using System;
using System.IO;
using ENode.Infrastructure;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

namespace ENode.Log4Net
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

            if (file.Exists)
            {
                XmlConfigurator.ConfigureAndWatch(file);
            }
            else
            {
                BasicConfigurator.Configure(new TraceAppender { Layout = new PatternLayout() });
            }
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
