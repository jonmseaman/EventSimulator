﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EventSimulator.Events;
using System.Text.RegularExpressions;

namespace EventSimulator
{
    public enum UserBehavior
    {
        Browsing,
        FastPurchase,
        SlowPurchase,
    }
    public class EventCreator
    {
        #region CreateEvents

        /// <summary>
        /// Creates a randomized click event.
        /// Randomized Variables: SessionId, Email, EntryTime, PrevUrl, NextUrl
        /// ExitTime is always DateTime.Now
        /// </summary>
        /// <returns> The randomized event. </returns>
        public static ClickEvent CreateClickEvent()
        {
            var e = new ClickEvent()
            {
                SessionId = Guid.NewGuid(),
                Email = RandomEmail(),
                EntryTime = DateTime.Now - TimeSpan.FromSeconds(Random.Next(1, 60 * 10)),
                ExitTime = DateTime.Now,
                PrevUrl = RandomUrl(),
                NextUrl = RandomUrl(),
            };
            return e;
        }

        /// <summary>
        /// Creates a randomized purchase event.
        /// TransactionNum, Email, ProductId, Price, and Quantity are all randomized.
        /// </summary>
        /// <returns> The randomly generate event. </returns>
        public static PurchaseEvent CreatePurchaseEvent()
        {
            var purchaseEvent = new PurchaseEvent();
            // TODO: Get the actual transaction number. Or, we could use Guid.
            purchaseEvent.SessionId = Guid.NewGuid();
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

        /// <summary>
        /// Creates a purchase that could follow from the previous event.
        /// </summary>
        /// <param name="event"></param>
        /// <exception cref="ArgumentException">Event must be a PurchaseEvent 
        /// or ClickEvent that corresponds to navigating to a product page.
        /// </exception>
        /// <returns></returns>
        public static PurchaseEvent CreateNextPurchaseEvent(Event @event)
        {
            // If PurchaseEvent, purchase again.
            if (@event is PurchaseEvent)
            {
                var nextEvent = new PurchaseEvent(@event as PurchaseEvent)
                {
                    Quantity = RandomProductQuantity(),
                    Time = DateTime.Now,
                };
                return nextEvent;
            }
            if (@event is ClickEvent
                     && IsUrlAProductPage(((ClickEvent)@event).NextUrl))
            {
                var clickEvent = (ClickEvent) @event;
                // Get the product id from the url.
                var productId = ProductIdFromUrl(clickEvent.NextUrl);
                var nextEvent = new PurchaseEvent(@event)
                {
                    Price = RandomPrice(),
                    ProductId = productId,
                    Quantity = RandomProductQuantity(),
                    Time = DateTime.Now,
                    TransactionNum = RandomTransactionNumber()
                };

                return nextEvent;
            }
            else
            {
                throw new ArgumentException("Event must be a PurchaseEvent "
                    + "or ClickEvent that corresponds to navigating to a "
                    + "product page.");
            }
        }


        /// <summary>
        /// Creates a click event that could come after @event.
        /// </summary>
        /// <param name="event">The event which precedes the event
        /// being created. Can be a purchase event or click event.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the event is
        /// not of type ClickEvent or PurchaseEvent.</exception>
        /// <returns>The created event. </returns>
        public static ClickEvent CreateNextClickEvent(Event @event)
        {
            var next = new ClickEvent(@event);

            // Get the type of the event
            if (@event is ClickEvent)
            {
                var old = (ClickEvent) @event;
                // Make the next event.
                next.PrevUrl = old.NextUrl;
                next.NextUrl = RandomUrl();
                next.EntryTime = old.ExitTime;
                next.ExitTime = DateTime.Now;
            }
            else if (@event is PurchaseEvent)
            {
                var old = (PurchaseEvent) @event;
                next.PrevUrl = ProductUrlFromId(old.ProductId);
                next.NextUrl = RandomUrl();
                next.EntryTime = old.Time;
                next.ExitTime = DateTime.Now;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            return next;
        }

        /// <summary>
        /// Creates an event that could be generated by an actual user which had just
        /// genrated 'prevEvent'.
        /// </summary>
        /// <param name="prevEvent">The 'previous' event.</param>
        /// <param name="behavior">The behavior of the simulated user.</param>
        /// <returns>An event that could occur after 'prevEvent'.</returns>
        public static Event CreateNextEvent(Event prevEvent, UserBehavior behavior)
        {
            if (prevEvent is PurchaseEvent)
            {
                return CreateClickEvent();
            }
            switch (behavior)
            {
                case UserBehavior.Browsing:
                    return CreateNextClickEvent(prevEvent);
                case UserBehavior.FastPurchase:
                    return CreateNextEventFastPurchase(prevEvent);
                case UserBehavior.SlowPurchase:
                    return CreateNextEventSlowPurchase(prevEvent);
            }
            return CreateClickEvent();
        }


        /// <summary>
        /// Slow purchase browses, but if the simulated user is on a product
        /// page, the user has a 50% chance to make the purchase.
        /// </summary>
        /// <param name="prevEvent">The previous event that was simulated.</param>
        /// <returns>The next event.</returns>
        private static Event CreateNextEventSlowPurchase(Event prevEvent)
        {
            // If ClickEvent && on Product page, 50% chance to purchase.
            if (prevEvent is ClickEvent
                && IsUrlAProductPage((prevEvent as ClickEvent).NextUrl)
                && Random.Next(0, 9) < 5)
            {
                return CreateNextPurchaseEvent(prevEvent);
            }
            else
            {
                return CreateNextClickEvent(prevEvent);
            }
        }

        #region CreateNextEvent specific behaviors

        /// <summary>
        /// Fast purchase navigates directly from the home page, then makes
        /// a purchase on the first product page that is reached.
        /// </summary>
        /// <param name="e">The previous event.</param>
        /// <returns>Event generated according to 'FastPurchase' behavior.</returns>
        private static Event CreateNextEventFastPurchase(Event e)
        {
            if (e is PurchaseEvent)
            {
                return CreateNextClickEvent(e);
            }

            ClickEvent clickEvent = new ClickEvent();
            if (e is ClickEvent)
            {
                clickEvent = (ClickEvent)e;
            }

            // If on a product page, purchase it.
            if (IsUrlAProductPage(clickEvent.NextUrl))
            {
                return CreateNextPurchaseEvent(clickEvent);
            }

            // Else, navigate to a product page.
            var next = new ClickEvent(e)
            {
                NextUrl = RandomProductUrl(),
                EntryTime = clickEvent.ExitTime,
                ExitTime = DateTime.Now,
                PrevUrl = clickEvent.NextUrl
            };
            return next;
        }

        #endregion


        #endregion

        #region Randoms
        /// <summary>
        /// Used to randomize click event members. Email, ProductUrl are randomized.
        /// </summary>
        private static Random Random = new Random();

        private static int RandomTransactionNumber()
        {
            return Random.Next(1, 1000000);
        }

        private static string RandomEmail()
        {
            return $"user{Random.Next(0, 99999)}@example.com";
        }

        private static int RandomProductId()
        {
            return Random.Next(1, 20);
        }

        private static int RandomPrice()
        {
            return Random.Next(25, 2500);
        }

        private static int RandomProductQuantity()
        {
            return Random.Next(1, 10);
        }

        private static string RandomUrl()
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

        private static string RandomProductUrl()
        {
            return $"{ProductPageUrl}{Random.Next(1, 20)}";
        }

        #endregion

        #region Utilities
        private static string HomePageUrl { get; } = "/";
        private static string ProductPageUrl { get; } = "/products/";

        public static string ProductUrlFromId(int productId)
        {
            return $"{ProductPageUrl}{productId}";
        }

        public static bool IsUrlAProductPage(string url)
        {
            return Regex.Match(url, $"({ProductPageUrl})[0-9]+").Length == url.Length;
        }

        public static bool IsUrlTheHomePage(string url)
        {
            return url == HomePageUrl;
        }

        /// <summary>
        /// Extracts the product id from a product page url.
        /// </summary>
        /// <param name="nextUrl">A url corresponding to a product page.</param>
        /// <exception cref="ArgumentException">Thrown if the url is not a product url.</exception>
        /// <returns></returns>
        public static int ProductIdFromUrl(string nextUrl)
        {
            if (!nextUrl.StartsWith(ProductPageUrl))
            {
                throw new ArgumentException("The url does not correspond to a product page.");
            }
            var prodIdStr = nextUrl.Substring(ProductPageUrl.Length);
            return int.Parse(prodIdStr);
        }

        #endregion

    }
}
