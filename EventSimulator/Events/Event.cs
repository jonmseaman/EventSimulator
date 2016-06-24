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

    public abstract class Event
    {
        #region Data Members
        public EventType EventType { get; set; }
        public Guid SessionId { get; set; }
        public string Email { get; set; }

        #endregion


        #region Constructors

        public Event(EventType eventType)
        {
            EventType = eventType;
        }

        /// <summary>
        /// Makes a shallow copy of e.
        /// </summary>
        /// <param name="e">The event to be copied. </param>
        public Event(Event e): this(e.EventType)
        {
            SessionId = e.SessionId;
            Email = e.Email;
        }

        #endregion
    }
}
