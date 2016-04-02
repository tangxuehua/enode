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
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class SqlServerCommandStore : ICommandStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _uniqueIndexName;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITypeNameProvider _typeNameProvider;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        #region Constructors

        /// <summary>Default constructor.
        /// </summary>
        public SqlServerCommandStore(OptionSetting optionSetting)
        {
            if (optionSetting != null)
            {
                _connectionString = optionSetting.GetOptionValue<string>("ConnectionString");
                _tableName = optionSetting.GetOptionValue<string>("TableName");
                _uniqueIndexName = optionSetting.GetOptionValue<string>("UniqueIndexName");
            }
            else
            {
                var setting = ENodeConfiguration.Instance.Setting.DefaultDBConfigurationSetting;
                _connectionString = setting.ConnectionString;
                _tableName = setting.CommandTableName;
                _uniqueIndexName = setting.CommandTableCommandIdUniqueIndexName;
            }

            Ensure.NotNull(_connectionString, "_connectionString");
            Ensure.NotNull(_tableName, "_tableName");
            Ensure.NotNull(_uniqueIndexName, "_uniqueIndexName");

            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        #endregion

        #endregion

        #region Public Methods

        public Task<AsyncTaskResult<CommandAddResult>> AddAsync(HandledCommand handledCommand)
        {
            var record = ConvertTo(handledCommand);

            return _ioHelper.TryIOFuncAsync(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.InsertAsync(record, _tableName);
                        return new AsyncTaskResult<CommandAddResult>(AsyncTaskStatus.Success, null, CommandAddResult.Success);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 && ex.Message.Contains(_uniqueIndexName))
                    {
                        return new AsyncTaskResult<CommandAddResult>(AsyncTaskStatus.Success, null, CommandAddResult.DuplicateCommand);
                    }
                    _logger.Error(string.Format("Add handled command has sql exception, handledCommand: {0}", handledCommand), ex);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Add handled command has unkown exception, handledCommand: {0}", handledCommand), ex);
                    throw;
                }
            }, "AddCommandAsync");
        }
        public Task<AsyncTaskResult<HandledCommand>> GetAsync(string commandId)
        {
            return _ioHelper.TryIOFuncAsync(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        var result = await connection.QueryListAsync<CommandRecord>(new { CommandId = commandId }, _tableName);
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
                CommandId = handledCommand.CommandId,
                AggregateRootId = handledCommand.AggregateRootId,
                MessagePayload = handledCommand.Message != null ? _jsonSerializer.Serialize(handledCommand.Message) : null,
                MessageTypeName = handledCommand.Message != null ? _typeNameProvider.GetTypeName(handledCommand.Message.GetType()) : null,
                CreatedOn = DateTime.Now,
            };
        }
        private HandledCommand ConvertFrom(CommandRecord record)
        {
            var message = default(IApplicationMessage);

            if (!string.IsNullOrEmpty(record.MessageTypeName))
            {
                var messageType = _typeNameProvider.GetType(record.MessageTypeName);
                message = _jsonSerializer.Deserialize(record.MessagePayload, messageType) as IApplicationMessage;
            }

            return new HandledCommand(record.CommandId, record.AggregateRootId, message);
        }

        #endregion

        class CommandRecord
        {
            public string CommandId { get; set; }
            public string AggregateRootId { get; set; }
            public string MessagePayload { get; set; }
            public string MessageTypeName { get; set; }
            public DateTime CreatedOn { get; set; }
        }
    }
}
