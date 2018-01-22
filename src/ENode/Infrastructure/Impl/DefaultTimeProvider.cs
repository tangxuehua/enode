using System;

namespace ENode.Infrastructure.Impl
{
    public class DefaultTimeProvider : ITimeProvider
    {
        public DateTime GetCurrentTime()
        {
            return DateTime.UtcNow;
        }
    }
}
