namespace ENode.Tests.Domain
{
    public class InheritTestAggregate : TestAggregate
    {
        public InheritTestAggregate(string id, string title) : base(id, title)
        {
        }

        public void ChangeMyTitle(string title)
        {
            ApplyEvent(new TestAggregateTitleChanged(title));
        }
    }
}
