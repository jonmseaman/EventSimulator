using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventSimulator.Events;

namespace EventSimulator
{
    public enum UserBehavior
    {
        Browsing,
        FastPurchase,
        SlowPurchase,
    }
    class EventCreator
    {
        public Event CreateClickEvent()
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

        public Event CreatePurchaseEvent()
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

        public List<Event> CreateUserEventSequence(UserBehavior behavior = UserBehavior.FastPurchase)
        {
            var list = new List<Event>();
            list.Add(CreateClickEvent());
            list.Add(CreateClickEvent());
            list.Add(CreateClickEvent());
            list.Add(CreateClickEvent());
            list.Add(CreatePurchaseEvent());
            // Data common across all of the events in the sequence


            // Make an event navigating to the home page.
            var e1 = new ClickEvent();
            e1.SessionId = Guid.NewGuid();
            e1.Email = "t-joseam@microsoft.com";
            e1.EntryTime = DateTime.Now - TimeSpan.FromMinutes(5);
            e1.ExitTime = DateTime.Now;
            e1.PrevUrl = null;
            e1.NextUrl = "/";
            // Make an event navigating to a product page.
            // Make an event that purchases the product.

            return list;
        }
    }
}
