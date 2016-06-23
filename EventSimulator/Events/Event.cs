using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSimulator.Events
{
    public enum EventType
    {
        Click,
        Purchase
    }

    public class Event
    {
        public Event(EventType eventType)
        {
            EventType = eventType;
        }
        public EventType EventType { get; set; }
        public Guid SessionId { get; set; }
        public string Email { get; set; }

        public static Event Create(EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Click:
                    return new ClickEvent();
                case EventType.Purchase:
                    return new PurchaseEvent();
                default:
                    return new Event(eventType);
            }
        }
    }
}
