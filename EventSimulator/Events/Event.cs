
namespace EventSimulator.Events
{
    using System;

    /// <summary>
    /// Represents an event generated from the ecommerce site.
    /// </summary>
    public abstract class Event
    {
        #region Data Members

        /// <summary>
        /// A unique id for the user's session. A session includes
        /// the series of click events and purchase events that are
        /// generated before a user signs off or stops using the site.
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The user's email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Used by back end system to identify the type of the event.
        /// </summary>
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
