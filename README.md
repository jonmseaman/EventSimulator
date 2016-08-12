# Event Hub Event Simulator
## Event Simulator Settings
 * Connection String - The connection string for the event hub. 
 * SendMode - This controls what type of events are sent by the Event Simulator.
   * ClickEvents - The app will only send Click events.
   * PurchaseEvents - The app will only send Purchase events.
   * SimulatedEvents - The app will send a combination of events. The user behavior will be simulated.
      *_User behaviors - The behavior of the simulated users. 
        * FastPurchase - The user will navigate directly from the home page to a product page, then purchase that item.
        * SlowPurchase - The user will navigate directly from the home page to a product page, but may navigate to other pages instead of buying that product.
        * Browsing - The user will browse the site, but will not make purchases.
 * Events per Second - The app will send this many events per second.
