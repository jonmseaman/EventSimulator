
namespace EventSimulator.SellerSite
{
    /// <summary>
    /// The user behaviors that the Event Creator can create
    /// events for.
    /// </summary>
    public enum UserBehavior
    {
        /// <summary>
        /// The user browses around pages, but does not make
        /// purchases.
        /// </summary>
        Browsing, 

        /// <summary>
        /// The user browses the site and purchases each
        /// product that the user comes to.
        /// </summary>
        FastPurchase, 

        /// <summary>
        /// The user browses the site and purchases some
        /// of the products that the users comes to.
        /// </summary>
        SlowPurchase, 
    }
}
