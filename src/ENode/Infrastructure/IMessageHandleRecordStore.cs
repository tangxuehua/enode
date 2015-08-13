using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Infrastructure
{
    /// <summary>Represents a storage to store the message handle records.
    /// </summary>
    public interface IMessageHandleRecordStore
    {
        /// <summary>Add a message handle record.
        /// </summary>
        /// <param name="record"></param>
        Task<AsyncTaskResult> AddRecordAsync(MessageHandleRecord record);
        /// <summary>Add a two-message handle record.
        /// </summary>
        /// <param name="record"></param>
        Task<AsyncTaskResult> AddRecordAsync(TwoMessageHandleRecord record);
        /// <summary>Add a three-message handle record.
        /// </summary>
        /// <param name="record"></param>
        Task<AsyncTaskResult> AddRecordAsync(ThreeMessageHandleRecord record);
        /// <summary>Check whether the message handle record exist.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="handlerTypeCode"></param>
        /// <param name="aggregateRootTypeCode"></param>
        /// <returns></returns>
        Task<AsyncTaskResult<bool>> IsRecordExistAsync(string messageId, int handlerTypeCode, int aggregateRootTypeCode);
        /// <summary>Check whether the two-message handle record exist.
        /// </summary>
        /// <param name="messageId1"></param>
        /// <param name="messageId2"></param>
        /// <param name="handlerTypeCode"></param>
        /// <param name="aggregateRootTypeCode"></param>
        /// <returns></returns>
        Task<AsyncTaskResult<bool>> IsRecordExistAsync(string messageId1, string messageId2, int handlerTypeCode, int aggregateRootTypeCode);
        /// <summary>Check whether the three-message handle record exist.
        /// </summary>
        /// <param name="messageId1"></param>
        /// <param name="messageId2"></param>
        /// <param name="messageId3"></param>
        /// <param name="handlerTypeCode"></param>
        /// <param name="aggregateRootTypeCode"></param>
        /// <returns></returns>
        Task<AsyncTaskResult<bool>> IsRecordExistAsync(string messageId1, string messageId2, string messageId3, int handlerTypeCode, int aggregateRootTypeCode);
    }
}
