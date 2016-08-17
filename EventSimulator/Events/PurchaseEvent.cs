using System;

namespace EventSimulator.Events
{
    public class PurchaseEvent : Event
    {

        #region Data Members

        public int TransactionNum { get; set; }
        public int ProductId { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
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
