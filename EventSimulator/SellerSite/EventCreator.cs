using System;
using System.Collections.Generic;
using System.IO;
using EventSimulator.Events;
using System.Text.RegularExpressions;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualBasic.FileIO;

namespace EventSimulator.SellerSite
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
        /// Randomized Variables: SessionId, Email, EntryTime, CurrentUrl, NextUrl
        /// ExitTime is always DateTime.Now
        /// </summary>
        /// <returns> The randomized event. </returns>
        public ClickEvent CreateClickEvent()
        {
            var rand = new Random();
            var e = new ClickEvent()
            {
                SessionId = Guid.NewGuid(),
                Email = SiteHelper.RandomEmail(),
                EntryTime = DateTime.Now - TimeSpan.FromSeconds(rand.Next(1, 60 * 10)),
                ExitTime = DateTime.Now,
                CurrentUrl = SiteHelper.RandomUrl(),
                NextUrl = SiteHelper.RandomUrl(),
            };
            return e;
        }

        /// <summary>
        /// Creates a randomized purchase event.
        /// TransactionNum, Email, ProductId, Price, and Quantity are all randomized.
        /// </summary>
        /// <returns> The randomly generate event. </returns>
        public PurchaseEvent CreatePurchaseEvent()
        {
            var purchaseEvent = new PurchaseEvent();
            // TODO: Get the actual transaction number. Or, we could use Guid.
            purchaseEvent.SessionId = Guid.NewGuid();
            purchaseEvent.TransactionNum = SiteHelper.RandomTransactionNumber();
            purchaseEvent.Email = SiteHelper.RandomEmail();
            purchaseEvent.ProductId = SiteHelper.RandomProductId();
            purchaseEvent.Price = SiteHelper.RandomPrice();
            purchaseEvent.Quantity = SiteHelper.RandomProductQuantity();
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
        public PurchaseEvent CreateNextPurchaseEvent(Event @event)
        {
            // If PurchaseEvent, purchase again.
            if (@event is PurchaseEvent)
            {
                var pEvent = (PurchaseEvent)@event;
                var nextEvent = new PurchaseEvent(pEvent)
                {
                    Quantity = SiteHelper.RandomProductQuantity(),
                    Time = pEvent.Time,
                };
                return nextEvent;
            }
            if (@event is ClickEvent
                     && SiteHelper.IsUrlAProductPage(((ClickEvent)@event).NextUrl))
            {
                var clickEvent = (ClickEvent)@event;
                // Get the product id from the url.
                var productId = SiteHelper.ProductIdFromUrl(clickEvent.NextUrl);
                var nextEvent = new PurchaseEvent(@event)
                {
                    ProductId = productId,
                    Price = SiteHelper.GetPrice(productId),
                    Quantity = SiteHelper.RandomProductQuantity(),
                    Time = DateTime.Now,
                    TransactionNum = SiteHelper.RandomTransactionNumber()
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
        public ClickEvent CreateNextClickEvent(Event @event)
        {
            var next = new ClickEvent(@event);

            // Get the type of the event
            if (@event is ClickEvent)
            {
                var old = (ClickEvent)@event;
                // Make the next event.
                next.CurrentUrl = old.NextUrl;
                next.NextUrl = SiteHelper.RandomUrl();
                next.EntryTime = old.ExitTime;
                next.ExitTime = DateTime.Now;
            }
            else if (@event is PurchaseEvent)
            {
                var old = (PurchaseEvent)@event;
                next.CurrentUrl = SiteHelper.ProductUrlFromId(old.ProductId);
                next.NextUrl = SiteHelper.RandomUrl();
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
        public Event CreateNextEvent(Event prevEvent, UserBehavior behavior)
        {
            Event nextEvent;

            switch (behavior)
            {
                case UserBehavior.Browsing:
                    nextEvent = CreateNextClickEvent(prevEvent);
                    break;
                case UserBehavior.FastPurchase:
                    nextEvent = CreateNextEventFastPurchase(prevEvent);
                    break;
                case UserBehavior.SlowPurchase:
                    nextEvent = CreateNextEventSlowPurchase(prevEvent);
                    break;
                default:
                    nextEvent = CreateClickEvent();
                    break;
            }
            return nextEvent;
        }

        #region CreateNextEvent specific behaviors

        /// <summary>
        /// Fast purchase navigates directly from the home page, then makes
        /// a purchase on the first product page that is reached.
        /// </summary>
        /// <param name="e">The previous event.</param>
        /// <returns>Event generated according to 'FastPurchase' behavior.</returns>
        private Event CreateNextEventFastPurchase(Event e)
        {
            Event nextEvent = CreateClickEvent();

            // 50% chance to purchase again
            if (e is PurchaseEvent && SiteHelper.Chance(50))
            {
                nextEvent = CreateNextPurchaseEvent(e);
            } else if (e is PurchaseEvent)
            {
                nextEvent = CreateNextClickEvent(e);
            }
            else if (e is ClickEvent && SiteHelper.IsUrlAProductPage(((ClickEvent)e).NextUrl))
            {
                nextEvent = CreateNextPurchaseEvent(e);
            }
            else if (e is ClickEvent)
            {
                var clickEvent = (ClickEvent) e;
                nextEvent = new ClickEvent(e)
                {
                    NextUrl = SiteHelper.RandomProductUrl(),
                    EntryTime = clickEvent.ExitTime,
                    ExitTime = DateTime.Now,
                    CurrentUrl = clickEvent.NextUrl
                };
            }

            return nextEvent;
        }

        /// <summary>
        /// Slow purchase browses, but if the simulated user is on a product
        /// page, the user has a 50% chance to make the purchase.
        /// </summary>
        /// <param name="prevEvent">The previous event that was simulated.</param>
        /// <returns>The next event.</returns>
        private Event CreateNextEventSlowPurchase(Event prevEvent)
        {
            Event nextEvent;

            // 50% chance to make another purchase.
            if (prevEvent is PurchaseEvent && SiteHelper.Chance(50))
            {
                nextEvent = CreateNextPurchaseEvent(prevEvent);
            }
            // 50% chance to buy an item when on a product page.
            else if (prevEvent is ClickEvent
                     && SiteHelper.IsUrlAProductPage(((ClickEvent)prevEvent).NextUrl)
                     && SiteHelper.Chance(50))
            {
                nextEvent = CreateNextPurchaseEvent(prevEvent);
            }
            else
            {
                nextEvent = CreateNextClickEvent(prevEvent);
            }

            return nextEvent;
        }

        #endregion


        #endregion

    }
}
