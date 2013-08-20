using System;

namespace ENode.Infrastructure.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public class EmptyLoggerFactory : ILoggerFactory
    {
        private static readonly EmptyLogger Logger = new EmptyLogger();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ILogger Create(string name)
        {
            return Logger;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ILogger Create(Type type)
        {
            return Logger;
        }
    }
}
