using System;
using ENode.Commanding;

namespace ENode.EQueue
{
    public interface ICommandTypeCodeProvider
    {
        int GetTypeCode(ICommand command);
        Type GetType(int typeCode);
    }
}
