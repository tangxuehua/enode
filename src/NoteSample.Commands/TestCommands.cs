using ENode.Commanding;

namespace NoteSample.Commands
{
    public class TestEventPriorityCommand : Command<string>
    {
    }
    public class ChangeNothingCommand : Command<string>
    {
    }
    public class ThrowExceptionCommand : Command<string>
    {
    }
    public class ThrowExceptionAsyncCommand : Command<string>
    {
    }
    public class NoHandlerCommand : Command<string>
    {
    }
    public class TwoHandlersCommand : Command<string>
    {
    }

    public class AsyncHandlerCommand : Command<string>
    {
    }
    public class AsyncHandlerCommand2 : Command<string>
    {
    }
    public class TwoAsyncHandlersCommand : Command<string>
    {
    }
    public class ChangeMultipleAggregatesCommand : Command<string>
    {
        public string AggregateRootId1 { get; set; }
        public string AggregateRootId2 { get; set; }
    }
}
