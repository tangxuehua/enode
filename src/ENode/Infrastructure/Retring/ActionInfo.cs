using System;

namespace ENode.Infrastructure.Retring
{
    /// <summary>A class contains the information of a specific retry action.
    /// </summary>
    public class ActionInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Func<object, bool> Action { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public object Data { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public ActionInfo Next { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="data"></param>
        /// <param name="next"></param>
        /// <exception cref="ArgumentNullException"></exception>
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
