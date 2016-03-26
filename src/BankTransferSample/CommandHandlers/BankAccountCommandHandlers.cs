using System.Threading.Tasks;
using BankTransferSample.ApplicationMessages;
using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ECommon.IO;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSample.CommandHandlers
{
    /// <summary>银行账户相关命令处理
    /// </summary>
    public class BankAccountCommandHandlers :
        ICommandHandler<CreateAccountCommand>,                       //开户
        ICommandAsyncHandler<ValidateAccountCommand>,                //验证账户是否合法
        ICommandHandler<AddTransactionPreparationCommand>,           //添加预操作
        ICommandHandler<CommitTransactionPreparationCommand>         //提交预操作
    {
        public bool CheckCommandHandledFirst
        {
            get { return true; }
        }

        public void Handle(ICommandContext context, CreateAccountCommand command)
        {
            context.Add(new BankAccount(command.AggregateRootId, command.Owner));
        }
        public Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(ValidateAccountCommand command)
        {
            var applicationMessage = default(ApplicationMessage);

            //此处应该会调用外部接口验证账号是否合法，这里仅仅简单通过账号是否以INVALID字符串开头来判断是否合法；根据账号的合法性，返回不同的应用层消息
            if (command.AggregateRootId.StartsWith("INVALID"))
            {
                applicationMessage = new AccountValidateFailedMessage(command.AggregateRootId, command.TransactionId, "账户不合法.");
            }
            else
            {
                applicationMessage = new AccountValidatePassedMessage(command.AggregateRootId, command.TransactionId);
            }

            return Task.FromResult(new AsyncTaskResult<IApplicationMessage>(AsyncTaskStatus.Success, applicationMessage));
        }
        public void Handle(ICommandContext context, AddTransactionPreparationCommand command)
        {
            context.Get<BankAccount>(command.AggregateRootId).AddTransactionPreparation(command.TransactionId, command.TransactionType, command.PreparationType, command.Amount);
        }
        public void Handle(ICommandContext context, CommitTransactionPreparationCommand command)
        {
            context.Get<BankAccount>(command.AggregateRootId).CommitTransactionPreparation(command.TransactionId);
        }
    }
}
