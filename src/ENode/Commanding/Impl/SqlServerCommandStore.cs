using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Dapper;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Configurations;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class SqlServerCommandStore : ICommandStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _commandTable;
        private readonly string _primaryKeyName;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITypeCodeProvider _typeCodeProvider;
        private readonly IEventSerializer _eventSerializer;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        public SqlServerCommandStore()
        {
            var setting = ENodeConfiguration.Instance.Setting.SqlServerCommandStoreSetting;
            Ensure.NotNull(setting, "SqlServerCommandStoreSetting");
            Ensure.NotNull(setting.ConnectionString, "SqlServerCommandStoreSetting.ConnectionString");
            Ensure.NotNull(setting.TableName, "SqlServerCommandStoreSetting.TableName");
            Ensure.NotNull(setting.PrimaryKeyName, "SqlServerCommandStoreSetting.PrimaryKeyName");

            _connectionString = setting.ConnectionString;
            _commandTable = setting.TableName;
            _primaryKeyName = setting.PrimaryKeyName;
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _typeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        #endregion

        #endregion

        #region Public Methods

        public CommandAddResult Add(HandledCommand handledCommand)
        {
            var record = ConvertTo(handledCommand);

            return _ioHelper.TryIOFunc(() =>
            {
                using (var connection = GetConnection())
                {
                    try
                    {
                        connection.Insert(record, _commandTable);
                        return CommandAddResult.Success;
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627)
                        {
                            if (ex.Message.Contains(_primaryKeyName))
                            {
                                return CommandAddResult.DuplicateCommand;
                            }
                        }
                        throw;
                    }
                }
            }, "AddCommand");
        }
        public void Remove(string commandId)
        {
            _ioHelper.TryIOAction(() =>
            {
                using (var connection = GetConnection())
                {
                    connection.Delete(new { CommandId = commandId }, _commandTable);
                }
            }, "RemoveCommand");
        }
        public HandledCommand Get(string commandId)
        {
            var record = _ioHelper.TryIOFunc(() =>
            {
                using (var connection = GetConnection())
                {
                    return connection.QueryList<CommandRecord>(new { CommandId = commandId }, _commandTable).SingleOrDefault();
                }
            }, "GetCommand");

            if (record != null)
            {
                return ConvertFrom(record);
            }
            return null;
        }

        public Task<AsyncTaskResult<CommandAddResult>> AddAsync(HandledCommand handledCommand)
        {
            var record = ConvertTo(handledCommand);

            return _ioHelper.TryIOFuncAsync<AsyncTaskResult<CommandAddResult>>(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.InsertAsync(record, _commandTable);
                        return new AsyncTaskResult<CommandAddResult>(AsyncTaskStatus.Success, null, CommandAddResult.Success);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 && ex.Message.Contains(_primaryKeyName))
                    {
                        return new AsyncTaskResult<CommandAddResult>(AsyncTaskStatus.Success, null, CommandAddResult.DuplicateCommand);
                    }
                    _logger.Error(string.Format("Add handled command has sql exception, handledCommand: {0}", handledCommand), ex);
                    return new AsyncTaskResult<CommandAddResult>(AsyncTaskStatus.IOException, ex.Message, CommandAddResult.Failed);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Add handled command has unkown exception, handledCommand: {0}", handledCommand), ex);
                    return new AsyncTaskResult<CommandAddResult>(AsyncTaskStatus.Failed, ex.Message, CommandAddResult.Failed);
                }
            }, "AddCommandAsync");
        }
        public Task<AsyncTaskResult> RemoveAsync(string commandId)
        {
            return _ioHelper.TryIOFuncAsync<AsyncTaskResult>(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.DeleteAsync(new { CommandId = commandId }, _commandTable);
                        return AsyncTaskResult.Success;
                    }
                }
                catch (SqlException ex)
                {
                    _logger.Error(string.Format("Remove handled command has sql exception, commandId: {0}", commandId), ex);
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Remove handled command has unkown exception, commandId: {0}", commandId), ex);
                    return new AsyncTaskResult(AsyncTaskStatus.Failed, ex.Message);
                }
            }, "RemoveCommandAsync");
        }
        public Task<AsyncTaskResult<HandledCommand>> GetAsync(string commandId)
        {
            return _ioHelper.TryIOFuncAsync<AsyncTaskResult<HandledCommand>>(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        var result = await connection.QueryListAsync<CommandRecord>(new { CommandId = commandId }, _commandTable);
                        var record = result.SingleOrDefault();
                        var handledCommand = record != null ? ConvertFrom(record) : null;
                        return new AsyncTaskResult<HandledCommand>(AsyncTaskStatus.Success, handledCommand);
                    }
                }
                catch (SqlException ex)
                {
                    _logger.Error(string.Format("Get handled command has sql exception, commandId: {0}", commandId), ex);
                    return new AsyncTaskResult<HandledCommand>(AsyncTaskStatus.IOException, ex.Message, null);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Get handled command has unkown exception, commandId: {0}", commandId), ex);
                    return new AsyncTaskResult<HandledCommand>(AsyncTaskStatus.Failed, ex.Message, null);
                }
            }, "GetCommandAsync");
        }

        #endregion

        #region Private Methods

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
        private CommandRecord ConvertTo(HandledCommand handledCommand)
        {
            return new CommandRecord
            {
                CommandId = handledCommand.Command.Id,
                CommandTypeCode = _typeCodeProvider.GetTypeCode(handledCommand.Command.GetType()),
                AggregateRootId = handledCommand.AggregateRootId,
                AggregateRootTypeCode = handledCommand.AggregateRootTypeCode,
                Timestamp = DateTime.Now,
                Payload = _jsonSerializer.Serialize(handledCommand.Command),
                Message = handledCommand.Message != null ? _jsonSerializer.Serialize(handledCommand.Message) : null,
                MessageTypeCode = handledCommand.Message != null ? _typeCodeProvider.GetTypeCode(handledCommand.Message.GetType()) : 0,
            };
        }
        private HandledCommand ConvertFrom(CommandRecord record)
        {
            var commandType = _typeCodeProvider.GetType(record.CommandTypeCode);
            var message = default(IApplicationMessage);

            if (record.MessageTypeCode > 0)
            {
                var messageType = _typeCodeProvider.GetType(record.MessageTypeCode);
                message = _jsonSerializer.Deserialize(record.Message, messageType) as IApplicationMessage;
            }

            return new HandledCommand(
                _jsonSerializer.Deserialize(record.Payload, commandType) as ICommand,
                record.AggregateRootId,
                record.AggregateRootTypeCode,
                message);
        }

        #endregion

        class CommandRecord
        {
            public string CommandId { get; set; }
            public int CommandTypeCode { get; set; }
            public int AggregateRootTypeCode { get; set; }
            public string AggregateRootId { get; set; }
            public DateTime Timestamp { get; set; }
            public string Payload { get; set; }
            public string Message { get; set; }
            public int MessageTypeCode { get; set; }
        }
    }
}
