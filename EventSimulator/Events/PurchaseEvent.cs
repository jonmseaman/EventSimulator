using System;

namespace EventSimulator.Events
{
    /// <summary>
    /// Represents a purchase on the ecommerce site. A purchase event
    /// only includes on item. When more than one item is purchased in
    /// a cart, more than one event is sent.
    /// </summary>
    public class PurchaseEvent : Event
    {
        #region Data Members

        /// <summary>
        /// A unique number identifying the transaction. Multiple purchase
        /// events with the same transaction number have the same number.
        /// </summary>
        public int TransactionNum { get; set; }

        /// <summary>
        /// An identification number unique to each product.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Product price per unit.
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Units purchased.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The time of the purchase.
        /// </summary>
        public DateTime Time { get; set; }

        #endregion

        #region Constructors
        public PurchaseEvent()
        {
            // Nothing to do here.
        }

        /// <summary>
        /// Makes a shallow copy of e.
        /// </summary>
        /// <param name="e">The event to be copied.</param>
        public PurchaseEvent(PurchaseEvent e) : base(e)
        {
            this.TransactionNum = e.TransactionNum;
            this.ProductId = e.ProductId;
            this.Price = e.Price;
            this.Quantity = e.Quantity;
            this.Time = e.Time;
        }

        /// <summary>
        /// Makes a shallow copy, but only copies inherited members.
        /// </summary>
        /// <param name="e"> The event to be copied.</param>
        public PurchaseEvent(Event e) : base(e)
        {
            // Nothing to do here.
        }


        #endregion
    }
}
