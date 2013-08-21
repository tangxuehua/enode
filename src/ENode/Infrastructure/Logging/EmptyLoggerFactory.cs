using System;

namespace ENode.Infrastructure.Logging
{
    /// <summary>An empty implementation of ILoggerFactory.
    /// </summary>
    public class EmptyLoggerFactory : ILoggerFactory
    {
        private static readonly EmptyLogger Logger = new EmptyLogger();
        /// <summary>Create an empty logger instance by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ILogger Create(string name)
        {
            return Logger;
        }
        /// <summary>Create an empty logger instance by type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ILogger Create(Type type)
        {
            return Logger;
        }
    }
}
