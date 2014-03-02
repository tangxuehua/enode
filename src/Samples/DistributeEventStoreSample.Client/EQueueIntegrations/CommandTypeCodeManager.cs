using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.IoC;
using ENode.Commanding;
using ENode.EQueue;
using DistributeEventStoreSample.Client.Commands;

namespace DistributeEventStoreSample.Client.EQueueIntegrations
{
    public class CommandTypeCodeManager : ICommandTypeCodeProvider
    {
        private IDictionary<int, Type> _typeCodeDict = new Dictionary<int, Type>();

        public CommandTypeCodeManager()
        {
            _typeCodeDict.Add(100, typeof(CreateNoteCommand));
            _typeCodeDict.Add(101, typeof(ChangeNoteTitleCommand));
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
