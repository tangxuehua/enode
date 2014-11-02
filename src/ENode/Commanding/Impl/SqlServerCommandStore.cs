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
        private readonly IBinarySerializer _binarySerializer;
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;

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
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<ICommand>>();
        }

        #endregion

        #endregion

        #region Public Methods

        public CommandAddResult Add(HandledCommand handledCommand)
        {
            var record = ConvertTo(handledCommand);

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
        }
        public void Remove(string commandId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                connection.Delete(new { CommandId = commandId }, _commandTable);
            }
        }
        public HandledCommand Get(string commandId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var record = connection.QueryList<CommandRecord>(new { CommandId = commandId }, _commandTable).SingleOrDefault();
                if (record != null)
                {
                    return ConvertFrom(record);
                }
                return null;
            }
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
                SourceEventId = handledCommand.SourceEventId,
                SourceExceptionId = handledCommand.SourceExceptionId,
                Timestamp = DateTime.Now,
                Payload = _binarySerializer.Serialize(handledCommand.Command),
                Events = _binarySerializer.Serialize(handledCommand.Events)
            };
        }
        private HandledCommand ConvertFrom(CommandRecord record)
        {
            var commandType = _commandTypeCodeProvider.GetType(record.CommandTypeCode);
            if (commandType == typeof(HandledAggregateCommand))
            {
                return new HandledAggregateCommand(
                    _binarySerializer.Deserialize<ICommand>(record.Payload),
                    record.SourceEventId,
                    record.SourceExceptionId,
                    record.AggregateRootId,
                    record.AggregateRootTypeCode);
            }
            else
            {
                return new HandledCommand(
                    _binarySerializer.Deserialize<ICommand>(record.Payload),
                    record.SourceEventId,
                    record.SourceExceptionId,
                    _binarySerializer.Deserialize<IEnumerable<IEvent>>(record.Events));
            }
        }

        #endregion

        class CommandRecord
        {
            public string CommandId { get; set; }
            public int CommandTypeCode { get; set; }
            public int AggregateRootTypeCode { get; set; }
            public string AggregateRootId { get; set; }
            public string SourceEventId { get; set; }
            public string SourceExceptionId { get; set; }
            public DateTime Timestamp { get; set; }
            public byte[] Payload { get; set; }
            public byte[] Events { get; set; }
        }
    }
}
