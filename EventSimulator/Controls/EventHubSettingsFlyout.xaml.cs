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
            this.InitializeComponent();
            this.Settings = settings;
        }

        private void CloseFlyout(object sender, RoutedEventArgs e)
        {
            this.IsOpen = false;
        }

        private void UpdateSettings(object sender, RoutedEventArgs e)
        {
            this.Settings.ConnectionString = this.ConnectionString.Text;

            int eventsPerSecond;
            if (int.TryParse(this.EventsPerSecond.Text, out eventsPerSecond))
            {
                this.Settings.EventsPerSecond = eventsPerSecond;
            }


            SendMode sendMode;
            if (Enum.TryParse(this.SendMode.Text, out sendMode))
            {
                this.Settings.SendMode = sendMode;
            }

            // Get behavior percents 
            int fastPurchasePercent;
            if (int.TryParse(this.TFastPurchase.Text, out fastPurchasePercent))
            {
                this.Settings.BehaviorPercents[0] = fastPurchasePercent;
            }

            int slowPurchasePercent;
            if (int.TryParse(this.TSlowPurchase.Text, out slowPurchasePercent))
            {
                this.Settings.BehaviorPercents[1] = slowPurchasePercent;
            }


            int browsingPercent;
            if (int.TryParse(this.TBrowsing.Text, out browsingPercent))
            {
                this.Settings.BehaviorPercents[2] = browsingPercent;
            }

            if (this.TabName.Text.Length == 0) this.TabName.Text = "New tab";
            this.CloseFlyout(sender, e);
        }
    }
}
