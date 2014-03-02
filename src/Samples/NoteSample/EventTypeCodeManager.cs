using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Eventing;
using NoteSample.DomainEvents;

namespace NoteSample
{
    public class EventTypeCodeManager : IEventTypeCodeProvider
    {
        private IDictionary<int, Type> _typeCodeDict = new Dictionary<int, Type>();

        public EventTypeCodeManager()
        {
            _typeCodeDict.Add(100, typeof(NoteCreatedEvent));
            _typeCodeDict.Add(101, typeof(NoteTitleChangedEvent));
        }

        public int GetTypeCode(IDomainEvent domainEvent)
        {
            return _typeCodeDict.Single(x => x.Value == domainEvent.GetType()).Key;
        }
        public Type GetType(int typeCode)
        {
            return _typeCodeDict[typeCode];
        }
    }
}
