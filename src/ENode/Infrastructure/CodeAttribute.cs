using System;

namespace ENode.Infrastructure
{
    /// <summary>An attribute to specify the code of type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CodeAttribute : Attribute
    {
        /// <summary>Represents the code of type.
        /// </summary>
        public int Code { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="code"></param>
        public CodeAttribute(int code)
        {
            Code = code;
        }
    }
}
