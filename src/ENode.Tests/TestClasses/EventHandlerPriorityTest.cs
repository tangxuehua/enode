using ECommon.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ENode.Commanding;
using System.Collections.Generic;
using ENode.Tests.Commands;

namespace ENode.Tests
{
    [TestClass]
    public class EventHandlerPriorityTest : BaseTest
    {
        public readonly static IList<string> HandlerTypes = new List<string>();

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Initialize(context);
        }

        [TestMethod]
        public void event_handler_priority_test()
        {
            var noteId = ObjectId.GenerateNewStringId();
            var command1 = new CreateTestAggregateCommand { AggregateRootId = noteId, Title = "Sample Title1" };
            var command2 = new TestEventPriorityCommand { AggregateRootId = noteId };
            _commandService.ExecuteAsync(command1, CommandReturnType.EventHandled).Wait();
            _commandService.ExecuteAsync(command2, CommandReturnType.EventHandled).Wait();

            Assert.AreEqual(9, HandlerTypes.Count);
            Assert.AreEqual(typeof(Handler3).Name, HandlerTypes[0]);
            Assert.AreEqual(typeof(Handler2).Name, HandlerTypes[1]);
            Assert.AreEqual(typeof(Handler1).Name, HandlerTypes[2]);
            Assert.AreEqual(typeof(Handler122).Name, HandlerTypes[3]);
            Assert.AreEqual(typeof(Handler121).Name, HandlerTypes[4]);
            Assert.AreEqual(typeof(Handler123).Name, HandlerTypes[5]);
            Assert.AreEqual(typeof(Handler1232).Name, HandlerTypes[6]);
            Assert.AreEqual(typeof(Handler1231).Name, HandlerTypes[7]);
            Assert.AreEqual(typeof(Handler1233).Name, HandlerTypes[8]);

            HandlerTypes.Clear();
        }
    }
}
