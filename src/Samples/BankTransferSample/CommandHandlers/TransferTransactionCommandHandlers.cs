using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;
using System.Threading.Tasks;

namespace BankTransferSample.CommandHandlers
{
    /// <summary>银行转账交易相关命令处理
    /// </summary>
    public class TransferTransactionCommandHandlers :
        ICommandHandler<StartTransferTransactionCommand>,                       //开始转账交易
        ICommandHandler<ConfirmAccountValidatePassedCommand>,                   //确认账户验证已通过
        ICommandHandler<ConfirmTransferOutPreparationCommand>,                  //确认预转出
        ICommandHandler<ConfirmTransferInPreparationCommand>,                   //确认预转入
        ICommandHandler<ConfirmTransferOutCommand>,                             //确认转出
        ICommandHandler<ConfirmTransferInCommand>,                              //确认转入
        ICommandHandler<CancelTransferTransactionCommand>                       //取消交易
    {
        public Task HandleAsync(ICommandContext context, StartTransferTransactionCommand command)
        {
            return context.AddAsync(new TransferTransaction(command.AggregateRootId, command.TransactionInfo));
        }
        public async Task HandleAsync(ICommandContext context, ConfirmAccountValidatePassedCommand command)
        {
            var transaction = await context.GetAsync<TransferTransaction>(command.AggregateRootId);
            transaction.ConfirmAccountValidatePassed(command.AccountId);
        }
        public async Task HandleAsync(ICommandContext context, ConfirmTransferOutPreparationCommand command)
        {
            var transaction = await context.GetAsync<TransferTransaction>(command.AggregateRootId);
            transaction.ConfirmTransferOutPreparation();
        }
        public async Task HandleAsync(ICommandContext context, ConfirmTransferInPreparationCommand command)
        {
            var transaction = await context.GetAsync<TransferTransaction>(command.AggregateRootId);
            transaction.ConfirmTransferInPreparation();
        }
        public async Task HandleAsync(ICommandContext context, ConfirmTransferOutCommand command)
        {
            var transaction = await context.GetAsync<TransferTransaction>(command.AggregateRootId);
            transaction.ConfirmTransferOut();
        }
        public async Task HandleAsync(ICommandContext context, ConfirmTransferInCommand command)
        {
            var transaction = await context.GetAsync<TransferTransaction>(command.AggregateRootId);
            transaction.ConfirmTransferIn();
        }
        public async Task HandleAsync(ICommandContext context, CancelTransferTransactionCommand command)
        {
            var transaction = await context.GetAsync<TransferTransaction>(command.AggregateRootId);
            transaction.Cancel();
        }
    }
}
