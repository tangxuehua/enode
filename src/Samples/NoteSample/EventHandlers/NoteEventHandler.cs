using System;
using ENode.Eventing;
using NoteSample.Events;

namespace NoteSample.EventSubscribers
{
    //这是一个事件订阅者，它也响应了Note的两个事件
    public class NoteEventHandler :
        IEventHandler<NoteCreated>,
        IEventHandler<NoteTitleChanged>
    {
        public void Handle(NoteCreated evnt)
        {
            //这里为了简单，所以只是输出了一串文字，实际我们可以在这里做任何你想做的事情；
            Console.WriteLine(string.Format("Note created, title：{0}", evnt.Title));
        }
        public void Handle(NoteTitleChanged evnt)
        {
            Console.WriteLine(string.Format("Note title changed, title：{0}", evnt.Title));
        }
    }
}
