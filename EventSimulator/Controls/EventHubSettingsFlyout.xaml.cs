using System;
using System.Windows;

namespace EventSimulator.Controls
{
    using Simulator;

    /// <summary>
    /// Interaction logic for EventHubSettingsFlyout.xaml
    /// </summary>
    public partial class EventHubSettingsFlyout : MahApps.Metro.Controls.Flyout
    {
        /// <summary>
        /// The settings object that the user's input will be written to.
        /// </summary>
        public Settings Settings { get; private set; }

        public EventHubSettingsFlyout(Settings settings)
        {
            InitializeComponent();
            Settings = settings;
        }

        private void CloseFlyout(object sender, RoutedEventArgs e)
        {
            IsOpen = false;
        }

        private void UpdateSettings(object sender, RoutedEventArgs e)
        {
            Settings.ConnectionString = ConnectionString.Text;

            int eventsPerSecond;
            if (int.TryParse(EventsPerSecond.Text, out eventsPerSecond))
            {
                Settings.EventsPerSecond = eventsPerSecond;
            }


            SendMode sendMode;
            if (Enum.TryParse(SendMode.Text, out sendMode))
            {
                Settings.SendMode = sendMode;
            }

            // Get behavior percents 
            int fastPurchasePercent;
            if (int.TryParse(TFastPurchase.Text, out fastPurchasePercent))
            {
                Settings.BehaviorPercents[0] = fastPurchasePercent;
            }

            int slowPurchasePercent;
            if (int.TryParse(TSlowPurchase.Text, out slowPurchasePercent))
            {
                Settings.BehaviorPercents[1] = slowPurchasePercent;
            }


            int browsingPercent;
            if (int.TryParse(TBrowsing.Text, out browsingPercent))
            {
                Settings.BehaviorPercents[2] = browsingPercent;
            }

            if (TabName.Text.Length == 0) TabName.Text = "New tab";
            CloseFlyout(sender, e);
        }
    }
}
