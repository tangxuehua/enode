using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Infrastructure
{
    /// <summary>Represents a storage to store the aggregate published version of aggregate.
    /// </summary>
    public interface IPublishedVersionStore
    {
        /// <summary>Update the published version for the given aggregate.
        /// </summary>
        Task<AsyncTaskResult> UpdatePublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId, int publishedVersion);
        /// <summary>Get the current published version for the given aggregate.
        /// </summary>
        Task<AsyncTaskResult<int>> GetPublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId);
    }
}
