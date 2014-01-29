using System;
using System.Collections.Generic;
using System.Linq;
using DistributeSample.Commands;
using ENode.Commanding;
using ENode.EQueue;

namespace DistributeSample.CommandProcessor.EQueueIntegrations
{
    public class CommandTypeCodeManager : ICommandTypeCodeProvider
    {
        private IDictionary<int, Type> _typeCodeDict = new Dictionary<int, Type>();

        public CommandTypeCodeManager()
        {
            _typeCodeDict.Add(100, typeof(CreateNoteCommand));
        }

        public int GetTypeCode(ICommand command)
        {
            return _typeCodeDict.Single(x => x.Value == command.GetType()).Key;
        }
        public Type GetType(int typeCode)
        {
            return _typeCodeDict[typeCode];
        }
    }
}
