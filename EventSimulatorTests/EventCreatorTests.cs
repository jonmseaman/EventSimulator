using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventSimulator;

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


            // Make sure nothing is null, and something is different.
            Assert.IsFalse(p1.TransactionNum.Equals(p2.TransactionNum)
                && p1.Email.Equals(p2.Email)
                && p1.ProductId.Equals(p2.ProductId)
                && p1.Price.Equals(p2.Price)
                && p1.Quantity.Equals(p2.Quantity));

            Assert.IsFalse(p1.SessionId == null);
        }
    }
}
