using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventSimulator;
using EventSimulator.Events;

namespace EventSimulatorTests
{
    [TestClass]
    public class EventCreatorTests
    {
        [TestMethod]
        public void CreateClickEventTest()
        {
            var c1 = EventCreator.CreateClickEvent();
            var c2 = EventCreator.CreateClickEvent();

            Assert.IsFalse(c1.SessionId.Equals(c2.SessionId));

            // Just make sure something is different about these events.
            // Chance that all these variables should be ~~ 0.
            Assert.IsFalse(c1.Email.Equals(c2.Email)
                && c1.PrevUrl.Equals(c2.PrevUrl)
                && c1.NextUrl.Equals(c2.NextUrl)
                && c1.EntryTime.Equals(c2.EntryTime)
                && c1.Email.Equals(c2.Email));
        }

        [TestMethod]
        public void CreatePurchaseEventTest()
        {
            var p1 = EventCreator.CreatePurchaseEvent();
            var p2 = EventCreator.CreatePurchaseEvent();

            Assert.IsFalse(p1.SessionId.Equals(p2.SessionId));

            // Make sure nothing is null, and something is different.
            Assert.IsFalse(p1.TransactionNum.Equals(p2.TransactionNum)
                && p1.Email.Equals(p2.Email)
                && p1.ProductId.Equals(p2.ProductId)
                && p1.Price.Equals(p2.Price)
                && p1.Quantity.Equals(p2.Quantity));
        }

        [TestMethod]
        public void CreateNextPurchaseEventWithPurchaseEventTest()
        {
            var first = EventCreator.CreatePurchaseEvent();
            var next = EventCreator.CreateNextPurchaseEvent(first);

            // Event Members
            Assert.AreEqual(first.SessionId, next.SessionId);
            Assert.AreEqual(first.Email, next.Email);
            // PurchaseEvent Members
            Assert.AreEqual(first.ProductId, next.ProductId);
            Assert.AreNotEqual(first.Time, next.Time);
        }

        [TestMethod]
        public void CreateNextPurchaseEventWithClickEventTest()
        {
            var first = EventCreator.CreateClickEvent();
            first.NextUrl = EventCreator.ProductUrlFromId(2);
            var next = EventCreator.CreateNextPurchaseEvent(first);

            Assert.AreEqual(2, next.ProductId);
            Assert.IsTrue(next.Price > 0);
            Assert.IsTrue(next.Quantity > 0);
            Assert.IsTrue(next.TransactionNum > 0);

        }

        [TestMethod]
        public void CreateNextClickEventFromClickEventTest()
        {
            var first = EventCreator.CreateClickEvent();
            var next = EventCreator.CreateNextClickEvent(first);

            // Check to make sure the urls are done correctly.
            Assert.AreEqual(first.NextUrl, next.PrevUrl);
            // first.PrevUrl does not have a relationship to next

            // Check to make sure that the dates have been updated.
            Assert.IsTrue(first.ExitTime <= next.EntryTime);
            Assert.IsTrue(next.ExitTime <= DateTime.Now);
        }

        [TestMethod]
        public void CreateNextClickEventFromPurchaseEventTest()
        {
            var p = EventCreator.CreatePurchaseEvent();
            var next = EventCreator.CreateNextClickEvent(p);

            // Check to make sure the urls are done correctly.
            Assert.AreEqual(EventCreator.ProductUrlFromId(p.ProductId), next.PrevUrl);
            // first.PrevUrl does not have a relationship to next

            // Check to make sure that the dates have been updated.
            Assert.IsTrue(p.Time <= next.EntryTime);
            Assert.IsTrue(next.ExitTime <= DateTime.Now);
        }

        [TestMethod]
        public void IsUrlAProductPageTrueTest()
        {
            Assert.IsTrue(EventCreator.IsUrlAProductPage("/products/2"));
        }

        [TestMethod]
        public void IsUrlAProductPageFalseTest()
        {
            Assert.IsFalse(EventCreator.IsUrlAProductPage("/products/2/hello"));
        }

        [TestMethod]
        public void IsUrlAProductPageFalse2Test()
        {
            Assert.IsFalse(EventCreator.IsUrlAProductPage("/notproducts/2/hello"));
        }

        [TestMethod]
        public void IsUrlAProductPageFalse3Test()
        {
            Assert.IsFalse(EventCreator.IsUrlAProductPage($"/"));
        }

    }
}
