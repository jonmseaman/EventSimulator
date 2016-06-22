using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventSimulator.Events;

namespace EventSimulator
{
    class EventCreator
    {
        public static Event CreateClickEvent()
        { // TODO: Randomize event data.
            var e = new ClickEvent();
            e.SessionId = Guid.NewGuid();
            e.Email = "t-joseam@microsoft.com";
            e.EntryTime = DateTime.Now - TimeSpan.FromMinutes(5);
            e.ExitTime = DateTime.Now;
            e.PrevUrl = "/";
            e.NextUrl = "/products/1";
            return e;
        }

        public static Event CreatePurchaseEvent()
        { // TODO: Randomize event data.
            var purchaseEvent = new PurchaseEvent();
            // TODO: Get the actual transaction number. Or, we could use Guid.
            purchaseEvent.TransactionNum = 1;
            purchaseEvent.Email = "t-joseam@microsoft.com";
            purchaseEvent.ProductId = 1.ToString();
            purchaseEvent.Price = 250;
            purchaseEvent.Quantity = 15;
            purchaseEvent.Time = DateTime.Now;
            return purchaseEvent;
        }
    }
}
