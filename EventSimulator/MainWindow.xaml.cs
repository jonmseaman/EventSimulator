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
            var s = new Settings
            {
                ConnectionString = ConnectionStringTextBox.Text
            };

            // Make the tab with those settings
            var eventHubControl = new EventHubControl()
            {
                Settings = s
            };
            var newTab = new TabItem()
            {
                Header = TabName.Text.Length > 0 ? TabName.Text : "EventHub",
                Content = eventHubControl
            };
            newTab.Focus();

            Tabs.Items.Add(newTab);
        }
    }
}
