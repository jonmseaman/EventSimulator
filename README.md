# Event Hub Event Simulator
## Event Simulator Settings
 * __Connection String__ - The connection string for the event hub. 
 * __SendMode__ - This controls what type of events are sent by the Event Simulator.
   * __ClickEvents__ - The app will only send Click events.
   * __PurchaseEvents__ - The app will only send Purchase events.
   * __SimulatedEvents__ - The app will send a combination of events. The user behavior will be simulated.
      * __User behaviors__ - The behavior of the simulated users. 
        * __FastPurchase__ - The user will navigate directly from the home page to a product page, then purchase that item.
        * __SlowPurchase__ - The user will navigate directly from the home page to a product page, but may navigate to other pages instead of buying that product.
        * __Browsing__ - The user will browse the site, but will not make purchases.
 * __Events per Second__ - The app will send this many events per second.
