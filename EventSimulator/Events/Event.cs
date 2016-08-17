using System;

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

        protected Event()
        {
        }

        /// <summary>
        /// Makes a shallow copy of e.
        /// </summary>
        /// <param name="e">The event to be copied. </param>
        protected Event(Event e)
        {
            this.SessionId = e.SessionId;
            this.Email = e.Email;
        }

        #endregion
    }
}
