using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EventSimulator.Events
{
    class ClickEvent : Event
    {
        public string PrevUrl { get; set; }
        public string NextUrl { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }

        public ClickEvent() : base(EventType.Click)
        { }

    }
}
