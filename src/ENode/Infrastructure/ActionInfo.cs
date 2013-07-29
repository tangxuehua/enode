using System;

namespace ENode.Infrastructure
{
    /// <summary>A class contains the information of a specific retry action.
    /// </summary>
    public class ActionInfo
    {
        public string Name { get; private set; }
        public Func<object, bool> Action { get; private set; }
        public object Data { get; private set; }
        public ActionInfo Next { get; private set; }

        public ActionInfo(string name, Func<object, bool> action, object data, ActionInfo next)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            Name = name;
            Action = action;
            Data = data;
            Next = next;
        }
    }
}
