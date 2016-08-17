using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

using EventSimulator.Simulator;

namespace EventSimulator.Controls
{
    using Simulator = EventSimulator.Simulator.Simulator;

    /// <summary>
    /// Interaction logic for EventHubControl.xaml
    /// </summary>
    public partial class EventHubControl : UserControl
    {
        /// <summary>
        /// The simulator.
        /// </summary>
        private readonly Simulator simulator;

        public EventHubSettingsFlyout SettingsFlyout { get; private set; }

        #region Constructor

        public EventHubControl(Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            InitializeComponent();
            this.simulator = new Simulator(settings);

            // List to Status change to update StartStopButton text
            this.simulator.PropertyChanged += SimulatorPropertyChanged;

            // Bind simulator status to GUI
            var statusBinding = new Binding("Status") { Source = this.simulator };
            TSimulatorStatus.SetBinding(TextBlock.TextProperty, statusBinding);

            // Bind events sent
            var eventsSentBinding = new Binding("EventsSent") { Source = this.simulator };
            TEventsSent.SetBinding(TextBlock.TextProperty, eventsSentBinding);

            // Bind events per second
            var epsBinding = new Binding("EventsPerSecond") { Source = this.simulator, StringFormat = "F2" };
            TEventsPerSecond.SetBinding(TextBlock.TextProperty, epsBinding);

            // Make the settings flyout
            this.SettingsFlyout = new EventHubSettingsFlyout(settings);
        }

        #endregion

        #region Events

        private void ToggleSimulatorSending(object sender, RoutedEventArgs e)
        {
            switch (this.simulator.Status)
            {
                case SimulatorStatus.Stopped:
                    new Thread(() => { this.simulator.StartSending(); }).Start();
                    break;
                case SimulatorStatus.Sending:
                    new Thread(() => { this.simulator.StopSending(); }).Start();
                    break;
                case SimulatorStatus.Stopping:

                    // Do nothing if SimulatorStatus.Stopping
                    break;
            }
        }

        private void ToggleSettingsFlyout(object sender, RoutedEventArgs args)
        {
            this.SettingsFlyout.IsOpen = !this.SettingsFlyout.IsOpen;
        }

        public void Shutdown(object sender, RoutedEventArgs args)
        {
            if (this.simulator.Status == SimulatorStatus.Sending)
            {
                new Thread(() =>
                {
                    this.simulator.StopSending();
                }).Start();
            }
        }

        public void SimulatorPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender != this.simulator || !args.PropertyName.Equals("Status")) return;
            
            if (Dispatcher.CheckAccess())
            {
                UpdateStartStopButton(this.simulator.Status);
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        UpdateStartStopButton(this.simulator.Status); 
                    }));
            }
        }

        #endregion

        private void UpdateStartStopButton(SimulatorStatus status)
        {
            switch (status)
            {
                case SimulatorStatus.Stopped:
                    StartStopButton.Content = "Start";
                    StartStopButton.Background = Brushes.Green;
                    break;
                case SimulatorStatus.Stopping:
                    StartStopButton.Content = "Stopping";
                    StartStopButton.Background = Brushes.DarkOrange;
                    break;
                case SimulatorStatus.Sending:
                    StartStopButton.Content = "Stop";
                    StartStopButton.Background = Brushes.Firebrick;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
