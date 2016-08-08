using System;
using System.Windows;

namespace EventSimulator.Controls
{
    /// <summary>
    /// Interaction logic for EventHubSettingsFlyout.xaml
    /// </summary>
    public partial class EventHubSettingsFlyout : MahApps.Metro.Controls.Flyout
    {
        public Simulator.Settings settings { get; private set; }

        public EventHubSettingsFlyout(Simulator.Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
        }

        private void CloseFlyout(object sender, RoutedEventArgs e)
        {
            this.IsOpen = false;
        }

        private void UpdateSettings(object sender, RoutedEventArgs e)
        {
            settings.ConnectionString = ConnectionString.Text;

            int eventsPerSecond;
            if (int.TryParse(EventsPerSecond.Text, out eventsPerSecond))
            {
                settings.EventsPerSecond = eventsPerSecond;
            }


            Simulator.SendMode sendMode;
            if (Enum.TryParse(SendMode.Text, out sendMode))
            {
                settings.SendMode = sendMode;
            }

            // Get behavior percents 
            int fastPurchasePercent;
            if (int.TryParse(TFastPurchase.Text, out fastPurchasePercent))
            {
                settings.BehaviorPercents[0] = fastPurchasePercent;
            }

            int slowPurchasePercent;
            if (int.TryParse(TSlowPurchase.Text, out slowPurchasePercent))
            {
                settings.BehaviorPercents[1] = slowPurchasePercent;
            }


            int browsingPercent;
            if (int.TryParse(TBrowsing.Text, out browsingPercent))
            {
                settings.BehaviorPercents[2] = browsingPercent;
            }

            if (TabName.Text.Length == 0) TabName.Text = "New tab";
            CloseFlyout(sender, e);
        }
    }
}
