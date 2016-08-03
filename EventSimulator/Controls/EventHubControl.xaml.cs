using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using EventSimulator.Simulator;
using System.Threading;
using System.Windows.Threading;

namespace EventSimulator.Controls
{
    /// <summary>
    /// Interaction logic for EventHubControl.xaml
    /// </summary>
    public partial class EventHubControl : UserControl
    {
        private readonly Simulator.Simulator _simulator;

        public EventHubSettingsFlyout settingsFlyout { get; private set; }

        #region Constructor

        public EventHubControl(Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            InitializeComponent();
            _simulator = new Simulator.Simulator(settings);

            // List to Status change to update StartStopButton text
            _simulator.PropertyChanged += SimulatorPropertyChanged;

            // Bind simulator status to GUI
            var statusBinding = new Binding("Status") { Source = _simulator };
            TSimulatorStatus.SetBinding(TextBlock.TextProperty, statusBinding);
            // Bind events sent
            var eventsSentBinding = new Binding("EventsSent") { Source = _simulator };
            TEventsSent.SetBinding(TextBlock.TextProperty, eventsSentBinding);
            // Bind events per second
            var epsBinding = new Binding("EventsPerSecond")
            {
                Source = _simulator,
                StringFormat = "F2"
            };
            TEventsPerSecond.SetBinding(TextBlock.TextProperty, epsBinding);

            // Make the settings flyout
            settingsFlyout = new EventHubSettingsFlyout(settings);
        }

        #endregion

        #region Events

        private void ToggleSimulatorSending(object sender, RoutedEventArgs e)
        {
            switch (_simulator.Status)
            {
                case SimulatorStatus.Stopped:
                    new Thread(() =>
                    {
                        _simulator.StartSending();
                    }).Start();
                    break;
                case SimulatorStatus.Sending:
                    new Thread(() =>
                    {
                        _simulator.StopSending();
                    }).Start();
                    break;
                case SimulatorStatus.Stopping:
                    // Do nothing if SimulatorStatus.Stopping
                    break;
            }
        }

        private void ToggleSettingsFlyout(object sender, RoutedEventArgs args)
        {
            settingsFlyout.IsOpen = !settingsFlyout.IsOpen;
        }

        public void Shutdown(object sender, RoutedEventArgs args)
        {
            if (_simulator.Status == SimulatorStatus.Sending)
            {
                new Thread(() =>
                {
                    _simulator.StopSending();
                }).Start();
            }
        }

        public void SimulatorPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender != _simulator || !args.PropertyName.Equals("Status")) return;
            
            if (Dispatcher.CheckAccess())
            {
                UpdateStartStopButton(_simulator.Status);
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                       UpdateStartStopButton(_simulator.Status); 
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
