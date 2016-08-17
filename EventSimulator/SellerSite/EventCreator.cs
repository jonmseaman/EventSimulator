
namespace EventSimulator.SellerSite
{
    using System;
    using Events;

    /// <summary>
    /// This class is used to create events which can later be sent to the
    /// event hub. 
    /// </summary>
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
            var purchaseEvent = new PurchaseEvent
            {
                SessionId = Guid.NewGuid(), 
                TransactionNum = SiteHelper.RandomTransactionNumber(), 
                Email = SiteHelper.RandomEmail(), 
                ProductId = SiteHelper.RandomProductId(), 
                Price = SiteHelper.RandomPrice(), 
                Quantity = SiteHelper.RandomProductQuantity(), 
                Time = DateTime.Now
            };
            return purchaseEvent;
        }

        #endregion

        #region CreateNextEvents

        /// <summary>
        /// Creates a purchase that could follow from the previous event.
        /// </summary>
        /// <param name="prevEvent">The previous event.</param>
        /// <returns>An event simulated which comes after <see cref="prevEvent"/>.</returns>
        public PurchaseEvent CreateNextPurchaseEvent(Event prevEvent)
        {
            // If PurchaseEvent, purchase again.
            var purchase = prevEvent as PurchaseEvent;
            if (purchase != null)
            {
                var nextEvent = new PurchaseEvent(purchase)
                {
                    Quantity = SiteHelper.RandomProductQuantity(), 
                    Time = purchase.Time
                };
                nextEvent.ProductId = SiteHelper.SimilarProductId(nextEvent.ProductId);
                return nextEvent;
            }

            var click = prevEvent as ClickEvent;
            if (click != null
                     && SiteHelper.IsUrlAProductPage(((ClickEvent)prevEvent).NextUrl))
            {
                //// Get the product id from the url.
                var productId = SiteHelper.ProductIdFromUrl(click.NextUrl);
                var nextEvent = new PurchaseEvent(click)
                {
                    ProductId = productId, 
                    Price = SiteHelper.GetPrice(productId), 
                    Quantity = SiteHelper.RandomProductQuantity(), 
                    Time = DateTime.Now, 
                    TransactionNum = SiteHelper.RandomTransactionNumber()
                };

                return nextEvent;
            }

            throw new NotImplementedException($"Cannot generate event for '{prevEvent.GetType()}'");
        }

        /// <summary>
        /// Creates a click event that could come after <see cref="prevEvent"/>.
        /// </summary>
        /// <param name="prevEvent">The event which precedes the event
        /// being created. Can be a purchase event or click event.</param>
        /// <returns>The event the comes after <see cref="prevEvent"/>.</returns>
        public ClickEvent CreateNextClickEvent(Event prevEvent)
        {
            var next = new ClickEvent(prevEvent);

            // Get the type of the event
            if (prevEvent is ClickEvent)
            {
                var old = (ClickEvent)prevEvent;

                //// Make the next event.
                next.CurrentUrl = old.NextUrl;
                next.NextUrl = SiteHelper.IsUrlTheHomePage(next.CurrentUrl)
                                   ? SiteHelper.RandomProductUrl()
                                   : SiteHelper.RandomUrl();
                next.EntryTime = old.ExitTime;
                next.ExitTime = DateTime.Now;
            }
            else if (prevEvent is PurchaseEvent)
            {
                var old = (PurchaseEvent)prevEvent;
                next.CurrentUrl = SiteHelper.ProductUrlFromId(old.ProductId);
                next.NextUrl = SiteHelper.RandomUrl();
                next.EntryTime = old.Time;
                next.ExitTime = DateTime.Now;
            }
            else
            {
                throw new NotImplementedException($"Cannot create next clickevent from '{prevEvent.GetType()}'");
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
                    nextEvent = this.CreateNextClickEvent(prevEvent);

                    // Limit the number of sesssions a user has.
                    if (SiteHelper.Chance(7)) nextEvent.Email = SiteHelper.RandomEmail();
                    break;
                case UserBehavior.FastPurchase:
                    nextEvent = this.CreateNextEventFastPurchase(prevEvent);
                    break;
                case UserBehavior.SlowPurchase:
                    nextEvent = this.CreateNextEventSlowPurchase(prevEvent);
                    break;
                default:
                    nextEvent = this.CreateClickEvent();
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
            Event nextEvent = this.CreateClickEvent();

            // 60% chance to purchase again
            if (e is PurchaseEvent && SiteHelper.Chance(60))
            {
                nextEvent = this.CreateNextPurchaseEvent(e);
            }
            else if (e is PurchaseEvent)
            {
                nextEvent = this.CreateNextClickEvent(e);
            }
            else if (e is ClickEvent && SiteHelper.IsUrlAProductPage(((ClickEvent)e).NextUrl))
            {
                nextEvent = this.CreateNextPurchaseEvent(e);
            }
            else if (e is ClickEvent)
            {
                var clickEvent = (ClickEvent)e;
                nextEvent = new ClickEvent(e)
                {
                    NextUrl = SiteHelper.RandomProductUrl(), 
                    EntryTime = clickEvent.ExitTime, 
                    ExitTime = DateTime.Now, 
                    CurrentUrl = clickEvent.NextUrl
                };
            }

            // Limit the number of sesssions a user has.
            if (e is PurchaseEvent && nextEvent is ClickEvent && SiteHelper.Chance(33))
            {
                nextEvent.Email = SiteHelper.RandomEmail();
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

            // 60% chance to make another purchase.
            if (prevEvent is PurchaseEvent && SiteHelper.Chance(60))
            {
                nextEvent = this.CreateNextPurchaseEvent(prevEvent);
            }

            // 50% chance to buy an item when on a product page.
            else if (prevEvent is ClickEvent && SiteHelper.IsUrlAProductPage(((ClickEvent)prevEvent).NextUrl)
                     && SiteHelper.Chance(50))
            {
                nextEvent = this.CreateNextPurchaseEvent(prevEvent);
            }
            else
            {
                nextEvent = this.CreateNextClickEvent(prevEvent);
            }

            // Limit the number of sesssions a user has.
            if (prevEvent is PurchaseEvent && nextEvent is ClickEvent && SiteHelper.Chance(50))
            {
                nextEvent.Email = SiteHelper.RandomEmail();
            }

            return nextEvent;
        }

        #endregion


        #endregion

    }
}
