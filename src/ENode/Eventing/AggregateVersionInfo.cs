using System;

namespace ENode.Eventing
{
    /// <summary>An structure contains the current version of the aggregate.
    /// </summary>
    public class AggregateVersionInfo
    {
        public const int Editing = 1;
        public const int UnEditing = 0;

        public int CurrentVersion = 0;
        public int Status = UnEditing;
    }
}
