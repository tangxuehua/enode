using System;
using ENode.Commanding;

namespace UniqueValidationSample.Commands
{
    [Serializable]
    public class RegisterUser : Command
    {
        public string UserName { get; set; }
    }
}
