using System;
using BankTransferSagaSample.Domain;
using BankTransferSagaSample.Events;
using ENode.Commanding;

namespace BankTransferSagaSample.Commands
{
    /// <summary>转账相关抽象命令基类
    /// </summary>
    [Serializable]
    public abstract class AbstractTransferCommand : Command
    {
        public Guid ProcessId { get; set; }
        public TransferInfo TransferInfo { get; set; }
    }
    /// <summary>发起转账
    /// </summary>
    [Serializable]
    public class StartTransfer : AbstractTransferCommand
    {
        public StartTransfer()
        {
            ProcessId = Guid.NewGuid();
        }
    }
    /// <summary>处理已转出事件
    /// </summary>
    [Serializable]
    public class HandleTransferedOut : AbstractTransferCommand
    {
    }
    /// <summary>处理已转入事件
    /// </summary>
    [Serializable]
    public class HandleTransferedIn : AbstractTransferCommand
    {
    }
    /// <summary>处理“转出失败”
    /// </summary>
    [Serializable]
    public class HandleFailedTransferOut : AbstractTransferCommand
    {
    }
    /// <summary>处理“转入失败”
    /// </summary>
    [Serializable]
    public class HandleFailedTransferIn : AbstractTransferCommand
    {
    }
    /// <summary>处理转出已回滚事件
    /// </summary>
    [Serializable]
    public class HandleTransferOutRolledback : AbstractTransferCommand
    {
    }
}
