using System;

namespace EventSimulator.Events
{
    public class ClickEvent : Event
    {
        public string CurrentUrl { get; set; }
        public string NextUrl { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }

        #region Constructors


        public ClickEvent()
        { }

        /// <summary>
        /// Makes a shallow copy of e.
        /// </summary>
        /// <param name="e">The event to be copied.</param>
        public ClickEvent(ClickEvent e) : base(e)
        {
            this.CurrentUrl = e.CurrentUrl;
            this.NextUrl = e.NextUrl;
            this.EntryTime = e.EntryTime;
            this.ExitTime = e.ExitTime;
        }

        /// <summary>
        /// Makes a shallow copy, but only copies inherited members.
        /// </summary>
        /// <param name="e"> The event to be copied.</param>
        public ClickEvent(Event e) : base(e)
        {
            // Nothing to do here.
        }



        #endregion
    }
}
