using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSimulator.Events
{
    public abstract class Event
    {
        #region Data Members
        public Guid SessionId { get; set; }
        public string Email { get; set; }

        public int EventType
        {
            get
            {
                var eventType = this is ClickEvent ? 1 : 2;
                return eventType;
            }
        }

        #endregion


        #region Constructors

        public Event()
        {
        }

        /// <summary>
        /// Makes a shallow copy of e.
        /// </summary>
        /// <param name="e">The event to be copied. </param>
        public Event(Event e)
        {
            SessionId = e.SessionId;
            Email = e.Email;
        }

        #endregion
    }
}
