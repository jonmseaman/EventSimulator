using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EventSimulator.Controls;
using EventSimulator.Simulator;

namespace EventSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ToggleSettingsFlyout()
        {
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
        }

        private void ToggleFlyout(object sender, RoutedEventArgs e)
        {
            ToggleSettingsFlyout();
        }

        private void CreateNewEventHubTab(object sender, RoutedEventArgs e)
        {
            // Make a new settings and copy information from flyout
            var tabName = TabName.Text;

            var connectionString = ConnectionString.Text;
            var settings = new Settings {ConnectionString = connectionString};

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

            // Make the tab with those settings
            var eventHubControl = new EventHubControl()
            {
                Settings = settings
            };
            var newTab = new TabItem()
            {
                Header = tabName.Length > 0 ? TabName.Text : (Tabs.Items.Count - 1).ToString(),
                Content = eventHubControl
            };


            SettingsFlyout.IsOpen = false;
            Tabs.Items.Add(newTab);
            newTab.Focus();
        }
    }
}
