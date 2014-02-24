using System;

namespace ENode.Domain
{
    /// <summary>Represents a domain execption.
    /// </summary>
    [Serializable]
    public class DomainException : Exception
    {
        /// <summary>The exception code.
        /// </summary>
        public int Code { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public DomainException(int code, string message) : base(string.Format("{0},ExceptionCode:{1}", message, code))
        {
            Code = code;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public DomainException(int code, string message, params object[] args) : base(string.Format("{0},ExceptionCode:{1}", string.Format(message, args), code))
        {
            Code = code;
        }
    }
}
