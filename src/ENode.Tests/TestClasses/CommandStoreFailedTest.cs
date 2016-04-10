using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Tests.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENode.Tests
{
    [TestClass]
    public class CommandStoreFailedTest : BaseTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Initialize(context, useMockCommandStore: true);
        }

        [TestMethod]
        public void duplicate_async_command_not_check_handler_exist_test()
        {
            var command = new NotCheckAsyncHandlerExistCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);

            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);

            ((MockCommandStore)_commandStore).SetExpectGetFailedCount(FailedType.TaskIOException, 5);
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockCommandStore)_commandStore).Reset();
        }
        [TestMethod]
        public void async_command_get_failed_test()
        {
            ((MockCommandStore)_commandStore).SetExpectGetFailedCount(FailedType.UnKnownException, 5);
            var asyncResult = _commandService.ExecuteAsync(new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockCommandStore)_commandStore).Reset();

            ((MockCommandStore)_commandStore).SetExpectGetFailedCount(FailedType.IOException, 5);
            asyncResult = _commandService.ExecuteAsync(new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockCommandStore)_commandStore).Reset();

            ((MockCommandStore)_commandStore).SetExpectGetFailedCount(FailedType.TaskIOException, 5);
            asyncResult = _commandService.ExecuteAsync(new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockCommandStore)_commandStore).Reset();
        }
        [TestMethod]
        public void async_command_add_failed_test()
        {
            ((MockCommandStore)_commandStore).SetExpectAddFailedCount(FailedType.UnKnownException, 5);
            var asyncResult = _commandService.ExecuteAsync(new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockCommandStore)_commandStore).Reset();

            ((MockCommandStore)_commandStore).SetExpectAddFailedCount(FailedType.IOException, 5);
            asyncResult = _commandService.ExecuteAsync(new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockCommandStore)_commandStore).Reset();

            ((MockCommandStore)_commandStore).SetExpectAddFailedCount(FailedType.TaskIOException, 5);
            asyncResult = _commandService.ExecuteAsync(new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockCommandStore)_commandStore).Reset();
        }
    }
}
