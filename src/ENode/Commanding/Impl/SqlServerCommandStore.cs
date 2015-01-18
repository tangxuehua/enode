using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using ECommon.Components;
using ECommon.Dapper;
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
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;
        private readonly IEventSerializer _eventSerializer;
        private readonly IOHelper _ioHelper;

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
            _commandTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<ICommand>>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
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
                    connection.Open();
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
                    connection.Open();
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
                    connection.Open();
                    return connection.QueryList<CommandRecord>(new { CommandId = commandId }, _commandTable).SingleOrDefault();
                }
            }, "GetCommand");

            if (record != null)
            {
                return ConvertFrom(record);
            }
            return null;
        }

        #endregion

        #region Private Methods

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
        private CommandRecord ConvertTo(HandledCommand handledCommand)
        {
            var handledAggregateCommand = handledCommand as HandledAggregateCommand;
            return new CommandRecord
            {
                CommandId = handledCommand.Command.Id,
                CommandTypeCode = _commandTypeCodeProvider.GetTypeCode(handledCommand.Command.GetType()),
                AggregateRootId = handledAggregateCommand != null ? handledAggregateCommand.AggregateRootId : null,
                AggregateRootTypeCode = handledAggregateCommand != null ? handledAggregateCommand.AggregateRootTypeCode : 0,
                SourceId = handledCommand.SourceId,
                SourceType = handledCommand.SourceType,
                Timestamp = DateTime.Now,
                CommandData = _jsonSerializer.Serialize(handledCommand.Command),
                Events = _jsonSerializer.Serialize(_eventSerializer.Serialize(handledCommand.Events))
            };
        }
        private HandledCommand ConvertFrom(CommandRecord record)
        {
            var commandType = _commandTypeCodeProvider.GetType(record.CommandTypeCode);
            if (commandType == typeof(HandledAggregateCommand))
            {
                return new HandledAggregateCommand(
                    _jsonSerializer.Deserialize(record.CommandData, commandType) as ICommand,
                    record.SourceId,
                    record.SourceType,
                    record.AggregateRootId,
                    record.AggregateRootTypeCode);
            }
            else
            {
                return new HandledCommand(
                    _jsonSerializer.Deserialize(record.CommandData, commandType) as ICommand,
                    record.SourceId,
                    record.SourceType,
                    _eventSerializer.Deserialize<IEvent>(_jsonSerializer.Deserialize<IDictionary<int, string>>(record.Events)));
            }
        }

        #endregion

        class CommandRecord
        {
            public string CommandId { get; set; }
            public int CommandTypeCode { get; set; }
            public int AggregateRootTypeCode { get; set; }
            public string AggregateRootId { get; set; }
            public string SourceId { get; set; }
            public string SourceType { get; set; }
            public DateTime Timestamp { get; set; }
            public string CommandData { get; set; }
            public string Events { get; set; }
        }
    }
}
