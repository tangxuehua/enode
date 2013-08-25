using System;

namespace ENode.Infrastructure.Retring
{
    /// <summary>A class contains the information of a specific retry action.
    /// </summary>
    public class ActionInfo
    {
        /// <summary>The name of the action.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>The action delegate.
        /// </summary>
        public Func<object, bool> Action { get; private set; }
        /// <summary>The parameter data of the action.
        /// </summary>
        public object Data { get; private set; }
        /// <summary>The next action of the current action. If the current action complete success, then the next action will be called.
        /// </summary>
        public ActionInfo Next { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="data"></param>
        /// <param name="next"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ActionInfo(string name, Func<object, bool> action, object data, ActionInfo next)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
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
