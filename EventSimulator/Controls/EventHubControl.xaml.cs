
namespace EventSimulator.Controls
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Simulator;
    using Simulator = Simulator.Simulator;

    /// <summary>
    /// Interaction logic for EventHubControl.xaml
    /// </summary>
    public partial class EventHubControl : UserControl
    {
        /// <summary>
        /// The simulator which generates and sends events to the event hub.
        /// </summary>
        private readonly Simulator _simulator;

        #region Constructor

        /// <summary>
        /// Setup the event hub control and settings flyout which will allow
        /// sending simulated events to an event hub. The user can change the
        /// settings passed to this constructor.
        /// </summary>
        /// <param name="settings">The settings for the simulator and event hub.</param>
        public EventHubControl(Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            InitializeComponent();
            _simulator = new Simulator(settings);

            // List to Status change to update StartStopButton text
            _simulator.PropertyChanged += SimulatorPropertyChanged;

            // Bind simulator status to GUI
            var statusBinding = new Binding("Status") { Source = _simulator };
            TSimulatorStatus.SetBinding(TextBlock.TextProperty, statusBinding);

            // Bind events sent
            var eventsSentBinding = new Binding("EventsSent") { Source = _simulator };
            TEventsSent.SetBinding(TextBlock.TextProperty, eventsSentBinding);

            // Bind events per second
            var epsBinding = new Binding("EventsPerSecond") { Source = _simulator, StringFormat = "F2" };
            TEventsPerSecond.SetBinding(TextBlock.TextProperty, epsBinding);

            // Make the settings flyout
            SettingsFlyout = new EventHubSettingsFlyout(settings);
        }

        #endregion

        /// <summary>
        /// The settings flyout for entering settings for the simulator used
        /// with this event hub control.
        /// </summary>
        public EventHubSettingsFlyout SettingsFlyout { get; private set; }

        /// <summary>
        /// Toggles whether the simulator is sending or not.
        /// May have a delay turning the simulator off.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleSimulatorSending(object sender, RoutedEventArgs e)
        {
            switch (_simulator.Status)
            {
                case SimulatorStatus.Stopped:
                    // TODO: Use task factory?
                    new Thread(() => { _simulator.StartSending(); }).Start();
                    break;
                case SimulatorStatus.Sending:
                    new Thread(() => { _simulator.StopSending(); }).Start();
                    break;
                case SimulatorStatus.Stopping:
                    // Do nothing if SimulatorStatus.Stopping
                    break;
            }
        }

        /// <summary>
        /// Opens or closes the flyout used t o change event hub settings.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="args">The parameter is not used.</param>
        public void ToggleSettingsFlyout(object sender, RoutedEventArgs args)
        {
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
        }

        /// <summary>
        /// Cause the simulator to start shutting down.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="args">The parameter is not used.</param>
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

        /// <summary>
        /// Updates the UI when a property of the <see cref="_simulator"/> changes.
        /// </summary>
        /// <param name="sender">The simulator that had a property change.</param>
        /// <param name="args">Not used.</param>
        public void SimulatorPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            // Only need to handle 'Status' change.
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

        /// <summary>
        /// Used to visually show the status of the simulator on the
        /// <see cref="StartStopButton"/>. Updates the color and text
        /// of the button.
        /// </summary>
        /// <param name="status">The status which will be shown on the button.</param>
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
