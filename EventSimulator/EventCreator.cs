using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
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
        #region Randoms
        /// <summary>
        /// Used to randomize the click events.
        /// </summary>
        private Random Random = new Random();

        public string HomePageUrl { get; private set; } = "/";

        private string RandomEmail()
        {
            return string.Format("user{0}@example.com", Random.Next(1, 99999));
        }

        private string RandomUrl()
        {
            var rand = Random.Next(0, 9);
            // 30% to go back to the home page.
            if (rand < 3)
            {
                return HomePageUrl;
            }
            else
            {
                return RandomProductUrl();
            }
        }

        private string RandomProductUrl()
        {
            return String.Format("/products/{0}", Random.Next(1, 20));
        }

        #endregion



        public Event CreateClickEvent()
        {
            var e = new ClickEvent();
            e.SessionId = Guid.NewGuid();
            e.Email = RandomEmail();
            e.EntryTime = DateTime.Now - TimeSpan.FromSeconds(Random.Next(1, 60 * 10));
            e.ExitTime = DateTime.Now;
            e.PrevUrl = RandomUrl();
            e.NextUrl = RandomProductUrl();
            return e;
        }

        public Event CreatePurchaseEvent()
        { // TODO: Randomize event data.
            var purchaseEvent = new PurchaseEvent();
            // TODO: Get the actual transaction number. Or, we could use Guid.
            purchaseEvent.TransactionNum = 1;
            purchaseEvent.Email = RandomEmail();
            purchaseEvent.ProductId = 1.ToString();
            purchaseEvent.Price = 250;
            purchaseEvent.Quantity = 15;
            purchaseEvent.Time = DateTime.Now;
            return purchaseEvent;
        }

        private PurchaseEvent CreateNextPurchaseEvent(Event @event)
        {
            throw new NotImplementedException();
        }

        private ClickEvent CreateNextClickEvent(Event @event)
        {
            var next = new ClickEvent();
            next.Email = @event.Email;
            next.SessionId = @event.SessionId;

            // Get the type of the event
            if (@event.EventType == EventType.Click)
            {
                var old = @event as ClickEvent;
                // Make the next event.
                // ClickEvent Members
                next.PrevUrl = old.NextUrl;
                next.NextUrl = RandomUrl();
                next.EntryTime = old.ExitTime;
                next.ExitTime = DateTime.Now;
            }
            else if (@event.EventType == EventType.Purchase)
            {
                var old = @event as PurchaseEvent;
                // ClickEvent Members
                // TODO: Make url(product id)
                next.PrevUrl = "/products/" + old.ProductId;
                next.NextUrl = RandomUrl();
                next.EntryTime = old.Time;
                next.ExitTime = DateTime.Now;
            }
            else
            {
                var err = String.Format("EventSimulator.Events.EventType does not have member {0}", @event.EventType);
                throw new InvalidEnumArgumentException(err);
            }

            return next;
        }

        // TODO: Clean up this method. Make a sub method for each
        // TODO: behavior.
        public Event CreateNextEvent(Event @event, UserBehavior behavior)
        {
            if (@event.EventType == EventType.Purchase)
            {
                return CreateClickEvent();
            }
            else if (behavior == UserBehavior.Browsing)
            {
                return CreateNextClickEvent(@event);
            }
            else if (behavior == UserBehavior.FastPurchase)
            {
                // If on product page, purchase
                var old = @event as ClickEvent;
                // If we are on a product page, purchase it.
                // TODO: IsProductPage(url) is better than this
                if (old.NextUrl != HomePageUrl)
                {
                    // If we are on a product page, we are
                    // going to purchase it.
                    return CreateNextPurchaseEvent(old);
                }
                else
                {
                    var next = CreateNextClickEvent(old);
                    next.NextUrl = RandomProductUrl();
                    return next;
                }
            }
            else if (behavior == UserBehavior.SlowPurchase)
            {
                if (Random.Next(0, 9) < 5) // 50%
                {
                    return CreateNextClickEvent(@event);
                }
                else
                {
                    return CreateNextPurchaseEvent(@event);
                }
            }
            else
            {
                return CreateClickEvent();
            }

        }


    }
}
