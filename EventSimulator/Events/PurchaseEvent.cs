using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSimulator.Events
{
    public class PurchaseEvent : Event
    {
        public int TransactionNum { get; set; }
        public string Email { get; set; }
        public string ProductId { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public DateTime Time { get; set; }

        public PurchaseEvent() : base(EventType.Purchase)
        {

        }
    }
}
