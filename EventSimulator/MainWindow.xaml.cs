using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using EventSimulator.Controls;
using EventSimulator.Simulator;

using MahApps.Metro.Controls;

namespace EventSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void CloseTabCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Remove tab from the main window.
        /// </summary>
        /// <param name="sender">The tab to remove.</param>
        /// <param name="e"></param>
        private void CloseTabCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // Stop sending the events.
            var tabItem = sender as EventHubControl;
            if (tabItem == null) return;
            this.Tabs.Items.Remove(tabItem);
            var flyout = tabItem.SettingsFlyout;
            this.Flyouts.Items.Remove(flyout);
        }

        /// <summary>
        /// Adds a tab to the main window that allows you to send to use
        /// an event simulator. <see cref="Simulator"/>.
        /// </summary>
        /// <param name="sender">Not used in method.</param>
        /// <param name="e">Not used in method.</param>
        private void CreateNewEventHubTab(object sender, RoutedEventArgs e)
        {
            // Setup the tab
            var eventHubControl = new EventHubControl(new Settings());

            var tabNameBinding = new Binding("Text")
            {
                Source = eventHubControl.SettingsFlyout.TabName, 
                Converter = new TabNameConverter()
            };
            var newTab = new MetroTabItem()
            {
                CloseButtonEnabled = true, 
                CloseTabCommand = ApplicationCommands.Close, 
                Header = "New tab", 
                Content = eventHubControl
            };
            newTab.SetBinding(HeaderedContentControl.HeaderProperty, tabNameBinding);
            newTab.Unloaded += eventHubControl.Shutdown;

            // Add tab
            this.Flyouts.Items.Add(eventHubControl.SettingsFlyout);
            this.Tabs.Items.Add(newTab);
            newTab.Focus();
            eventHubControl.SettingsFlyout.IsOpen = true;
        }


        /// <summary>
        /// Converts a string to a tab name. Will convert to a default
        /// value 'New tab' if the supplied value is null or empty. 
        /// </summary>
        public class TabNameConverter : IValueConverter
        {
            /// <summary>
            /// Converts an empty string value to 'New tab'.
            /// Otherwise, leaves the value unchanged.
            /// </summary>
            /// <param name="value">The string to convert.</param>
            /// <returns>The converted tab name.</returns>
            public object Convert(object value, Type targetType, 
                object parameter, CultureInfo culture)
            {
                // Do the conversion from bool to visibility
                var tabName = value as string;
                if (string.IsNullOrEmpty(tabName))
                {
                    tabName = "New tab";
                }

                return tabName;
            }

            /// <summary>
            /// Reverse conversion. Does not change the value.
            /// </summary>
            /// <returns>Returns the parameter 'value'.</returns>
            public object ConvertBack(object value, Type targetType, 
                object parameter, CultureInfo culture)
            {
                // Do the conversion from visibility to bool
                return value;
            }
        }

    }
}
