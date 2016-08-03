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
using MahApps.Metro;

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

        private void CloseTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Stop sending the events.
            var tabItem = sender as EventHubControl;
            if (tabItem == null) return;
            Tabs.Items.Remove(tabItem);
            var flyout = tabItem.settingsFlyout;
            this.Flyouts.Items.Remove(flyout);
        }

        /// <summary>
        /// Adds a tab to the main window that allows you to send to use
        /// an event simulator. <see cref="Simulator.Simulator"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewEventHubTab(object sender, RoutedEventArgs e)
        {
            // Setup the tab
            var eventHubControl = new EventHubControl(new Settings());

            var tabNameBinding = new Binding("Text")
            {
                Source = eventHubControl.settingsFlyout.TabName
            };
            var newTab = new MetroTabItem()
            {
                CloseButtonEnabled = true,
                CloseTabCommand = ApplicationCommands.Close,
                Header = "New tab",
                Content = eventHubControl
            };
            newTab.SetBinding(TabItem.HeaderProperty, tabNameBinding);
            newTab.Unloaded += eventHubControl.Shutdown;

            // Add tab
            var flyoutsControl = new FlyoutsControl();
            Flyouts.Items.Add(eventHubControl.settingsFlyout);
            Tabs.Items.Add(newTab);
            newTab.Focus();
            eventHubControl.settingsFlyout.IsOpen = true;
        }
    }
}
