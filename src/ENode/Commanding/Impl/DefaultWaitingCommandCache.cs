using System;
using System.Collections.Generic;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IProcessingCommandCache.
    /// </summary>
    public class DefaultWaitingCommandCache : IWaitingCommandCache
    {
        private readonly IDictionary<object, int> _processingCommandCountDict = new Dictionary<object, int>();
        private readonly IDictionary<object, Queue<ICommand>> _waitingCommandDict = new Dictionary<object, Queue<ICommand>>();

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
        /// <param name="command"></param>
        public bool AddWaitingCommand(object aggregateRootId, ICommand command)
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
                    if (!_waitingCommandDict.ContainsKey(aggregateRootId))
                    {
                        _waitingCommandDict.Add(aggregateRootId, new Queue<ICommand>());
                    }
                    _waitingCommandDict[aggregateRootId].Enqueue(command);
                    return true;
                }
                return false;
            }
        }
        /// <summary>Try to fetch a waiting command for the specified aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public ICommand FetchWaitingCommand(object aggregateRootId)
        {
            lock (this)
            {
                if (_processingCommandCountDict.ContainsKey(aggregateRootId))
                {
                    ICommand command = null;
                    _processingCommandCountDict[aggregateRootId] -= 1;
                    if (_processingCommandCountDict[aggregateRootId] > 0)
                    {
                        command = _waitingCommandDict[aggregateRootId].Dequeue();
                    }
                    else
                    {
                        _processingCommandCountDict.Remove(aggregateRootId);
                        _waitingCommandDict.Remove(aggregateRootId);
                    }
                    return command;
                }
                return null;
            }
        }
    }
}
