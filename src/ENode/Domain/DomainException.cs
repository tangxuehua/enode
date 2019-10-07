using System;
using System.Collections.Generic;
using ECommon.Utilities;

namespace ENode.Domain
{
    public abstract class DomainException : Exception, IDomainException
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public IDictionary<string, string> Items { get; set; }

        public DomainException()
        {
            Id = ObjectId.GenerateNewStringId();
            Timestamp = DateTime.UtcNow;
            Items = new Dictionary<string, string>();
        }

        public abstract void SerializeTo(IDictionary<string, string> serializableInfo);
        public abstract void RestoreFrom(IDictionary<string, string> serializableInfo);

        public void MergeItems(IDictionary<string, string> items)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }
            if (Items == null)
            {
                Items = new Dictionary<string, string>();
            }
            foreach (var entry in items)
            {
                if (!Items.ContainsKey(entry.Key))
                {
                    Items.Add(entry.Key, entry.Value);
                }
            }
        }
    }
}
