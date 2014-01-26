using System.Collections.Generic;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IProcessingCommandCache.
    /// </summary>
    public class DefaultWaitingCommandCache : IWaitingCommandCache
    {
        private readonly IDictionary<object, int> _processingCommandCountDict = new Dictionary<object, int>();
        private readonly IDictionary<object, Queue<ProcessingCommand>> _processingCommandQueueDict = new Dictionary<object, Queue<ProcessingCommand>>();

        /// <summary>Try to add a waiting command for the specified aggregate.
        /// <remarks>
        /// First, increase the processing command count of the given aggregate. Then check:
        /// if the aggregate has at least one command which was processing:
        /// 1. initiate a new waiting queue for the current aggregate;
        /// 2. enqueue the command to the initiated waiting queue;
        /// 3. set the initiated waiting queue to the current aggregate.
        /// 4. returns true;
        /// else:
        /// 2. returns false;
        /// </remarks>
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="processingCommand"></param>
        public bool AddWaitingCommand(object aggregateRootId, ProcessingCommand processingCommand)
        {
            lock (this)
            {
                if (_processingCommandCountDict.ContainsKey(aggregateRootId))
                {
                    _processingCommandCountDict[aggregateRootId] += 1;
                }
                else
                {
                    _processingCommandCountDict.Add(aggregateRootId, 1);
                }
                if (_processingCommandCountDict[aggregateRootId] > 1)
                {
                    if (!_processingCommandQueueDict.ContainsKey(aggregateRootId))
                    {
                        _processingCommandQueueDict.Add(aggregateRootId, new Queue<ProcessingCommand>());
                    }
                    _processingCommandQueueDict[aggregateRootId].Enqueue(processingCommand);
                    return true;
                }
                return false;
            }
        }
        /// <summary>Try to fetch a waiting command for the specified aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public ProcessingCommand FetchWaitingCommand(object aggregateRootId)
        {
            lock (this)
            {
                if (_processingCommandCountDict.ContainsKey(aggregateRootId))
                {
                    ProcessingCommand processingCommand = null;
                    _processingCommandCountDict[aggregateRootId] -= 1;
                    if (_processingCommandCountDict[aggregateRootId] > 0)
                    {
                        processingCommand = _processingCommandQueueDict[aggregateRootId].Dequeue();
                    }
                    else
                    {
                        _processingCommandCountDict.Remove(aggregateRootId);
                        _processingCommandQueueDict.Remove(aggregateRootId);
                    }
                    return processingCommand;
                }
                return null;
            }
        }
    }
}
