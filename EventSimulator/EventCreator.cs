using System;
using System.Collections.Generic;
using System.IO;
using EventSimulator.Events;
using System.Text.RegularExpressions;
using MathNet.Numerics.Distributions;
using Microsoft.VisualBasic.FileIO;

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
        #region Private Members

        private List<string[]> productData = new List<string[]>();

        private const int _priceIndex = 4;

        #endregion

        #region Constructor

        public EventCreator()
        {
            // TODO: Load product data from a file.
            try
            {
                // Load product data from file
                var parser = new TextFieldParser(new StreamReader("data/products.csv"));
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                // Load data while can
                while (!parser.EndOfData)
                {
                    // Add that data to our list
                    productData.Add(parser.ReadFields());
                }
            }
            catch (Exception)
            {
                // TODO: Better catch statement.
                // If we failed to load the data from the file.
                productData = null;
            }

        }

        #endregion

        #region CreateEvents

        /// <summary>
        /// Creates a randomized click event.
        /// Randomized Variables: SessionId, Email, EntryTime, CurrentUrl, NextUrl
        /// ExitTime is always DateTime.Now
        /// </summary>
        /// <returns> The randomized event. </returns>
        public ClickEvent CreateClickEvent()
        {
            var e = new ClickEvent()
            {
                SessionId = Guid.NewGuid(),
                Email = RandomEmail(),
                EntryTime = DateTime.Now - TimeSpan.FromSeconds(Random.Next(1, 60 * 10)),
                ExitTime = DateTime.Now,
                CurrentUrl = RandomUrl(),
                NextUrl = RandomUrl(),
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
        public PurchaseEvent CreateNextPurchaseEvent(Event @event)
        {
            // If PurchaseEvent, purchase again.
            if (@event is PurchaseEvent)
            {
                var pEvent = @event as PurchaseEvent;
                var nextEvent = new PurchaseEvent(pEvent)
                {
                    Quantity = RandomProductQuantity(),
                    Time = pEvent.Time,
                };
                return nextEvent;
            }
            if (@event is ClickEvent
                     && IsUrlAProductPage(((ClickEvent)@event).NextUrl))
            {
                var clickEvent = (ClickEvent)@event;
                // Get the product id from the url.
                var productId = ProductIdFromUrl(clickEvent.NextUrl);
                var nextEvent = new PurchaseEvent(@event)
                {
                    // TODO: Get the price according to the product id and data from csv
                    ProductId = productId,
                    Price = GetPrice(productId),
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
        public ClickEvent CreateNextClickEvent(Event @event)
        {
            var next = new ClickEvent(@event);

            // Get the type of the event
            if (@event is ClickEvent)
            {
                var old = (ClickEvent)@event;
                // Make the next event.
                next.CurrentUrl = old.NextUrl;
                next.NextUrl = RandomUrl();
                next.EntryTime = old.ExitTime;
                next.ExitTime = DateTime.Now;
            }
            else if (@event is PurchaseEvent)
            {
                var old = (PurchaseEvent)@event;
                next.CurrentUrl = ProductUrlFromId(old.ProductId);
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
        public Event CreateNextEvent(Event prevEvent, UserBehavior behavior)
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

        #region CreateNextEvent specific behaviors

        /// <summary>
        /// Fast purchase navigates directly from the home page, then makes
        /// a purchase on the first product page that is reached.
        /// </summary>
        /// <param name="e">The previous event.</param>
        /// <returns>Event generated according to 'FastPurchase' behavior.</returns>
        private Event CreateNextEventFastPurchase(Event e)
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
                CurrentUrl = clickEvent.NextUrl
            };
            return next;
        }

        /// <summary>
        /// Slow purchase browses, but if the simulated user is on a product
        /// page, the user has a 50% chance to make the purchase.
        /// </summary>
        /// <param name="prevEvent">The previous event that was simulated.</param>
        /// <returns>The next event.</returns>
        private Event CreateNextEventSlowPurchase(Event prevEvent)
        {
            // If ClickEvent && on Product page, 50% chance to purchase.
            if (prevEvent is ClickEvent
                && IsUrlAProductPage(((ClickEvent)prevEvent).NextUrl)
                && Random.Next(0, 9) < 5)
            {
                return CreateNextPurchaseEvent(prevEvent);
            }
            else
            {
                return CreateNextClickEvent(prevEvent);
            }
        }

        #endregion


        #endregion

        #region Randoms
        /// <summary>
        /// Used to randomize click event members. Email, ProductUrl are randomized.
        /// </summary>
        private static readonly Random Random = new Random();

        private static int RandomTransactionNumber()
        {
            return Random.Next(1, 1000000);
        }

        private static string RandomEmail()
        {
            var rand = (int)Normal.Sample(Random, 25000, 7500);
            return $"user{rand}@example.com";
        }

        private static int RandomProductId()
        {
            var rand = (int)Normal.Sample(Random, 7.0, 1.5);
            rand = rand < 1 ? 1 : rand;
            rand = rand > 16 ? 16 : rand;
            return rand;
        }

        private static int RandomPrice()
        {
            var rand = (int)Normal.Sample(Random, 1250, 250);
            rand = rand < 250 ? 250 : rand;
            return rand;
        }

        private int GetPrice(int productId)
        {
            int price = 0;
            try
            {
                // If productData isn't null
                if (productData != null)
                {
                    // Must convert to price in cents.
                    double dPrice;
                    double.TryParse(productData[productId][_priceIndex], out dPrice);
                    price = (int) Math.Round(100.0 * dPrice);
                }
            }
            catch (IndexOutOfRangeException)
            {

            }
            if (price == 0)
            {
                price = RandomPrice();
            }
            return price;
        }

        private static int RandomProductQuantity()
        {
            var rand = (int)Normal.Sample(Random, 5, 2);
            rand = rand < 1 ? 1 : rand;
            return rand;
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
            return $"{ProductPageUrl}{RandomProductId()}";
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
        private static int ProductIdFromUrl(string nextUrl)
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
