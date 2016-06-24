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
            var ec = new EventCreator();
            var c1 = ec.CreateClickEvent();
            var c2 = ec.CreateClickEvent();

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
            var ec = new EventCreator();
            var p1 = ec.CreatePurchaseEvent();
            var p2 = ec.CreatePurchaseEvent();

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
            var ec = new EventCreator();

            var first = ec.CreatePurchaseEvent();
            var next = ec.CreateNextPurchaseEvent(first);

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
            var ec = new EventCreator();

            var first = ec.CreateClickEvent();
            // TODO: Fix this for if the product url changes.
            first.NextUrl = "/products/2";
            var next = ec.CreateNextPurchaseEvent(first);

            Assert.AreEqual(2, next.ProductId);
            Assert.IsTrue(next.Price > 0);
            Assert.IsTrue(next.Quantity > 0);
            Assert.IsTrue(next.TransactionNum > 0);

        }
    }
}
