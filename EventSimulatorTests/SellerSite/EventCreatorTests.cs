using System;
using System.CodeDom;
using System.Collections.Generic;

using EventSimulator.Events;
using EventSimulator.SellerSite;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventSimulatorTests
{
    [TestClass]
    public class EventCreatorTests
    {
        EventCreator eventCreator = new EventCreator();

        const int NumTriesForRandoms = 15;

        #region CreateEventsTests
        [TestMethod]
        public void CreateClickEventTest()
        {
            var c1 = eventCreator.CreateClickEvent();
            var c2 = eventCreator.CreateClickEvent();

            Assert.IsFalse(c1.SessionId.Equals(c2.SessionId));

            // Just make sure something is different about these events.
            // Chance that all these variables should be ~~ 0.
            Assert.IsFalse(c1.Email.Equals(c2.Email)
                && c1.CurrentUrl.Equals(c2.CurrentUrl)
                && c1.NextUrl.Equals(c2.NextUrl)
                && c1.EntryTime.Equals(c2.EntryTime)
                && c1.Email.Equals(c2.Email));
        }

        [TestMethod]
        public void CreatePurchaseEventTest()
        {
            var p1 = eventCreator.CreatePurchaseEvent();
            var p2 = eventCreator.CreatePurchaseEvent();

            Assert.IsFalse(p1.SessionId.Equals(p2.SessionId));

            // Make sure nothing is null, and something is different.
            Assert.IsFalse(p1.TransactionNum.Equals(p2.TransactionNum)
                && p1.Email.Equals(p2.Email)
                && p1.ProductId.Equals(p2.ProductId)
                && p1.Price.Equals(p2.Price)
                && p1.Quantity.Equals(p2.Quantity));
        }

        #endregion

        #region CreateNextPurchaseEventTests
        [TestMethod]
        public void CreateNextPurchaseEventWithPurchaseEventTest()
        {
            var first = eventCreator.CreatePurchaseEvent();
            var next = eventCreator.CreateNextPurchaseEvent(first);

            // Event Members
            Assert.AreEqual(first.SessionId, next.SessionId);
            Assert.AreEqual(first.Email, next.Email);

            // PurchaseEvent Members
            Assert.AreEqual(first.ProductId, next.ProductId);
            Assert.AreEqual(first.Time, next.Time);
        }

        [TestMethod]
        public void CreateNextPurchaseEventWithClickEventTest()
        {
            var first = eventCreator.CreateClickEvent();
            first.NextUrl = SiteHelper.ProductUrlFromId(2);
            var next = eventCreator.CreateNextPurchaseEvent(first);

            Assert.AreEqual(2, next.ProductId);
            Assert.IsTrue(next.Price > 0);
            Assert.IsTrue(next.Quantity > 0);
            Assert.IsTrue(next.TransactionNum > 0);

        }

        #endregion

        #region CreateNextClickEventTests
        [TestMethod]
        public void CreateNextClickEventFromClickEventTest()
        {
            var first = eventCreator.CreateClickEvent();
            var next = eventCreator.CreateNextClickEvent(first);

            // Check to make sure the urls are done correctly.
            Assert.AreEqual(first.NextUrl, next.CurrentUrl);

            // first.CurrentUrl does not have a relationship to next

            // Check to make sure that the dates have been updated.
            Assert.IsTrue(first.ExitTime <= next.EntryTime);
            Assert.IsTrue(next.ExitTime <= DateTime.Now);
        }

        [TestMethod]
        public void CreateNextClickEventFromPurchaseEventTest()
        {
            var p = eventCreator.CreatePurchaseEvent();
            var next = eventCreator.CreateNextClickEvent(p);

            // Check to make sure the urls are done correctly.
            Assert.AreEqual(SiteHelper.ProductUrlFromId(p.ProductId), next.CurrentUrl);

            // first.CurrentUrl does not have a relationship to next

            // Check to make sure that the dates have been updated.
            Assert.IsTrue(p.Time <= next.EntryTime);
            Assert.IsTrue(next.ExitTime <= DateTime.Now);
        }

        #endregion

        #region CreateNextEventTests

        [TestMethod]
        public void CreateNextEvent_ClickEvent_Browsing()
        {
            for (int i = 0; i < NumTriesForRandoms; i++)
            {
                var first = eventCreator.CreateClickEvent();
                var next = eventCreator.CreateNextEvent(first, UserBehavior.Browsing) as ClickEvent;

                Assert.IsNotNull(next);
                Assert.AreEqual(first.NextUrl, next.CurrentUrl);

                // Check to make sure the urls are done correctly.
                Assert.AreEqual(first.NextUrl, next.CurrentUrl);

                // first.CurrentUrl does not have a relationship to next

                // Check to make sure that the dates have been updated.
                Assert.IsTrue(first.ExitTime <= next.EntryTime);
                Assert.IsTrue(next.ExitTime <= DateTime.Now);
            }
        }

        [TestMethod]
        public void CreateNextEvent_PurchaseEvent_Browsing()
        {
            var first = eventCreator.CreatePurchaseEvent();
            var next = eventCreator.CreateNextEvent(first, UserBehavior.Browsing) as ClickEvent;

            Assert.IsNotNull(next);

            // Check to make sure that the dates have been updated.
            Assert.IsTrue(first.Time <= next.EntryTime);
            Assert.IsInstanceOfType(next, typeof(ClickEvent));
            Assert.IsTrue(next.ExitTime <= DateTime.Now);
        }

        [TestMethod]
        public void CreateNextEvent_ClickEventNotOnProductPage_FastPurchase()
        {
            // When not on the product page, the next event should be a click event on a product page
            var first = eventCreator.CreateClickEvent();
            first.CurrentUrl = "/notonaproductpage/";
            first.NextUrl = "/notonaproductpage/";

            var next = eventCreator.CreateNextEvent(first, UserBehavior.FastPurchase) as ClickEvent;
            Assert.IsNotNull(next);
            Assert.IsTrue(SiteHelper.IsUrlAProductPage(next.NextUrl));
        }

        [TestMethod]
        public void CreateNextEvent_ClickEventOnProductPage_FastPurchase()
        {
            // This should result in a purchase event being generated.
            var first = eventCreator.CreateClickEvent();
            first.NextUrl = SiteHelper.RandomProductUrl();

            var next = eventCreator.CreateNextEvent(first, UserBehavior.FastPurchase) as PurchaseEvent;
            Assert.IsNotNull(next);
            Assert.AreEqual(SiteHelper.ProductIdFromUrl(first.NextUrl), next.ProductId, "Product Ids did not match.");
        }

        [TestMethod]
        public void CreateNextEvent_PurchaseEvent_FastPurchase()
        {
            // This should sometimes generate a product event.
            var foundPurchase = false;
            for (int i = 0; i < NumTriesForRandoms; i++)
            {
                var first = eventCreator.CreatePurchaseEvent();
                var next = eventCreator.CreateNextEvent(first, UserBehavior.FastPurchase);

                var pNext = next as PurchaseEvent;
                if (pNext != null)
                {
                    Assert.AreEqual(first.TransactionNum, pNext.TransactionNum);
                    Assert.AreEqual(first.Time, pNext.Time);
                }

                foundPurchase = foundPurchase || next is PurchaseEvent;
            }

            Assert.IsTrue(foundPurchase);
        }

        [TestMethod]
        public void CreateNextEvent_ClickEventOnProductPage_SlowPurchase()
        {
            // CreateNextEvent should sometimes generate a purchase event on a product page.
            var foundPurchase = false;
            var foundClick = false;

            for (int i = 0; i < NumTriesForRandoms; i++)
            {
                var first = eventCreator.CreateClickEvent();
                first.NextUrl = SiteHelper.RandomProductUrl();

                var next = eventCreator.CreateNextEvent(first, UserBehavior.SlowPurchase);
                foundPurchase = foundPurchase || next is PurchaseEvent;
                foundClick = foundClick || next is ClickEvent;
            }

            Assert.IsTrue(foundPurchase);
            Assert.IsTrue(foundClick);
        }

        [TestMethod]
        public void CreateNextEvent_PurchaseEvent_SlowPurchase()
        {
            // CreateNextEvent should sometimes generate another purchase event.
            var foundPurchase = false;
            var foundClick = false;

            for (int i = 0; i < NumTriesForRandoms; i++)
            {
                var first = eventCreator.CreatePurchaseEvent();
                var next = eventCreator.CreateNextEvent(first, UserBehavior.SlowPurchase);

                foundPurchase = foundPurchase || next is PurchaseEvent;
                foundClick = foundClick || next is ClickEvent;
            }

            Assert.IsTrue(foundPurchase);
            Assert.IsTrue(foundClick);

        }

        #endregion

    }
}
