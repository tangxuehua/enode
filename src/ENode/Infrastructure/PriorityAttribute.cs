using System;

namespace ENode.Infrastructure
{
    /// <summary>An attribute to specify the priority of message handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PriorityAttribute : Attribute
    {
        /// <summary>The priority value.
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>Default constructor.
        /// </summary>
        public PriorityAttribute() : this(0) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="priority"></param>
        public PriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
