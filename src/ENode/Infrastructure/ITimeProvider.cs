using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a time provider.
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>Get the current time.
        /// </summary>
        /// <returns></returns>
        DateTime GetCurrentTime();
    }
}
