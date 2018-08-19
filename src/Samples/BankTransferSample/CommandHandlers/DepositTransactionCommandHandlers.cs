using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;
using System.Threading.Tasks;

namespace BankTransferSample.CommandHandlers
{
    /// <summary>银行存款交易相关命令处理
    /// </summary>
    public class DepositTransactionCommandHandlers :
        ICommandHandler<StartDepositTransactionCommand>,                      //开始交易
        ICommandHandler<ConfirmDepositPreparationCommand>,                    //确认预存款
        ICommandHandler<ConfirmDepositCommand>                                //确认存款
    {
        public Task HandleAsync(ICommandContext context, StartDepositTransactionCommand command)
        {
            context.Add(new DepositTransaction(command.AggregateRootId, command.AccountId, command.Amount));
            return Task.CompletedTask;
        }
        public async Task HandleAsync(ICommandContext context, ConfirmDepositPreparationCommand command)
        {
            var transaction = await context.GetAsync<DepositTransaction>(command.AggregateRootId);
            transaction.ConfirmDepositPreparation();
        }
        public async Task HandleAsync(ICommandContext context, ConfirmDepositCommand command)
        {
            var transaction = await context.GetAsync<DepositTransaction>(command.AggregateRootId);
            transaction.ConfirmDeposit();
        }
    }
}
