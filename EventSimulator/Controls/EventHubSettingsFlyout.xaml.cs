using System;
using System.Windows;

namespace EventSimulator.Controls
{
    using EventSimulator.Simulator;

    /// <summary>
    /// Interaction logic for EventHubSettingsFlyout.xaml
    /// </summary>
    public partial class EventHubSettingsFlyout : MahApps.Metro.Controls.Flyout
    {
        public Settings Settings { get; private set; }

        public EventHubSettingsFlyout(Settings settings)
        {
            InitializeComponent();
            this.Settings = settings;
        }

        private void CloseFlyout(object sender, RoutedEventArgs e)
        {
            IsOpen = false;
        }

        private void UpdateSettings(object sender, RoutedEventArgs e)
        {
            this.Settings.ConnectionString = ConnectionString.Text;

            int eventsPerSecond;
            if (int.TryParse(EventsPerSecond.Text, out eventsPerSecond))
            {
                this.Settings.EventsPerSecond = eventsPerSecond;
            }


            SendMode sendMode;
            if (Enum.TryParse(SendMode.Text, out sendMode))
            {
                this.Settings.SendMode = sendMode;
            }

            // Get behavior percents 
            int fastPurchasePercent;
            if (int.TryParse(TFastPurchase.Text, out fastPurchasePercent))
            {
                this.Settings.BehaviorPercents[0] = fastPurchasePercent;
            }

            int slowPurchasePercent;
            if (int.TryParse(TSlowPurchase.Text, out slowPurchasePercent))
            {
                this.Settings.BehaviorPercents[1] = slowPurchasePercent;
            }


            int browsingPercent;
            if (int.TryParse(TBrowsing.Text, out browsingPercent))
            {
                this.Settings.BehaviorPercents[2] = browsingPercent;
            }

            if (TabName.Text.Length == 0) TabName.Text = "New tab";
            CloseFlyout(sender, e);
        }
    }
}
