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
        #region CreateEvents

        /// <summary>
        /// Creates a randomized click event. 
        /// </summary>
        /// <returns> The randomized event. </returns>
        public Event CreateClickEvent()
        {
            var e = new ClickEvent();
            e.SessionId = Guid.NewGuid();
            e.Email = RandomEmail();
            e.EntryTime = DateTime.Now - TimeSpan.FromSeconds(Random.Next(1, 60 * 10));
            e.ExitTime = DateTime.Now;
            e.PrevUrl = RandomUrl();
            e.NextUrl = IsUrlTheHomePage(e.PrevUrl) ? RandomProductUrl() : RandomUrl();
            return e;
        }

        /// <summary>
        /// Creates a randomized purchase event.
        /// TransactionNum, Email, ProductId, Price, and Quantity are all randomized.
        /// 
        /// </summary>
        /// <returns> The randomly generate event. </returns>
        public Event CreatePurchaseEvent()
        { // TODO: Randomize event data.
            var purchaseEvent = new PurchaseEvent();
            // TODO: Get the actual transaction number. Or, we could use Guid.
            purchaseEvent.TransactionNum = RandomTransactionNumber();
            purchaseEvent.Email = RandomEmail();
            purchaseEvent.ProductId = RandomProductId();
            purchaseEvent.Price = RandomPrice();
            purchaseEvent.Quantity = RandomProductQuantity();
            purchaseEvent.Time = DateTime.Now;
            return purchaseEvent;
        }

        #endregion

        #region CreateNextEvents

        private PurchaseEvent CreateNextPurchaseEvent(Event @event)
        {
            // If PurchaseEvent, purchase again.
            if (@event.EventType == EventType.Purchase)
            {
                var nextEvent = new PurchaseEvent(@event as PurchaseEvent);
                nextEvent.Quantity = RandomProductQuantity();
            }
            else if (@event.EventType == EventType.Click
                && IsUrlAProductPage((@event as ClickEvent).NextUrl))
            { // If click event
                var nextEvent = new PurchaseEvent(@event);

            }
            // If clickEvent && onProductPage
                // PurchaseProduct
            // If on Home page
                // Throw exception
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
                else if (/* On product page */IsUrlAProductPage((@event as ClickEvent).NextUrl))
                {
                    return CreateNextPurchaseEvent(@event);
                }
            }
            else
            {
                return CreateClickEvent();
            }

        }

        #endregion

        #region Randoms
        /// <summary>
        /// Used to randomize click event members. Email, ProductUrl are randomized.
        /// </summary>
        private Random Random = new Random();

        private int RandomTransactionNumber()
        {
            return Random.Next(1, 1000000);
        }

        private string RandomEmail()
        {
            return $"user{Random.Next(0, 99999)}@example.com";
        }

        private int RandomProductId()
        {
            return Random.Next(1, 20);
        }

        private int RandomPrice()
        {
            return Random.Next(25, 2500);
        }

        private int RandomProductQuantity()
        {
            return Random.Next(1, 10);
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
            return $"{ProductPageUrl}{Random.Next(1, 20)}";
        }

        #endregion

        #region Utilities
        private string HomePageUrl { get; } = "/";
        private string ProductPageUrl { get; } = "/products/";


        bool IsUrlAProductPage(string url)
        {
            return url.StartsWith(ProductPageUrl);
        }

        bool IsUrlTheHomePage(string url)
        {
            return url == HomePageUrl;
        }

        #endregion

    }
}
