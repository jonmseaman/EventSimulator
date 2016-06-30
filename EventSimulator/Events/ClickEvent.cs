using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

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
        public ClickEvent(ClickEvent e): base(e)
        {
            CurrentUrl = e.CurrentUrl;
            NextUrl = e.NextUrl;
            EntryTime = e.EntryTime;
            ExitTime = e.ExitTime;
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
