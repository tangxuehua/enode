using System;
using ENode.Domain;
using ENode.Eventing;
using NoteSample.Events;

namespace NoteSample.Domain
{
    [Serializable]
    public class Note : AggregateRoot<Guid>,
        IEventHandler<NoteCreated>,     //订阅事件
        IEventHandler<NoteTitleChanged>
    {
        public string Title { get; private set; }
        public DateTime CreatedTime { get; private set; }
        public DateTime UpdatedTime { get; private set; }

        public Note() : base() { }
        public Note(Guid id, string title) : base(id)
        {
            var currentTime = DateTime.Now;
            //触发事件
            RaiseEvent(new NoteCreated(Id, title, currentTime, currentTime));
        }

        public void ChangeTitle(string title)
        {
            //触发事件
            RaiseEvent(new NoteTitleChanged(Id, title, DateTime.Now));
        }

        //事件响应函数
        void IEventHandler<NoteCreated>.Handle(NoteCreated evnt)
        {
            //在响应函数中修改自己的状态，这里可以体现出EDA的影子，就是事件驱动状态的修改
            Title = evnt.Title;
            CreatedTime = evnt.CreatedTime;
            UpdatedTime = evnt.UpdatedTime;
        }
        //事件响应函数
        void IEventHandler<NoteTitleChanged>.Handle(NoteTitleChanged evnt)
        {
            //同上解释
            Title = evnt.Title;
            UpdatedTime = evnt.UpdatedTime;
        }
    }
}
