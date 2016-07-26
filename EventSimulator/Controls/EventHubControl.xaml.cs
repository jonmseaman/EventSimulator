using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using EventSimulator.Simulator;
using System.Threading;
using System.Windows.Threading;
using System.Xaml;

namespace EventSimulator.Controls
{
    /// <summary>
    /// Interaction logic for EventHubControl.xaml
    /// </summary>
    public partial class EventHubControl : UserControl
    {
        private readonly Simulator.Simulator _simulator;

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
        }

        #endregion

        private void StartStopButton_OnClick(object sender, RoutedEventArgs e)
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

        private void UpdateStartStopButton(SimulatorStatus status)
        {
            switch (status)
            {
                case SimulatorStatus.Stopped:
                    StartStopButton.Content = "Start";
                    StartStopButton.Background = Brushes.Green;
                    break;
                case SimulatorStatus.Stopping:
                    //StartStopButton.Content = "Stopping";
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
    }
}
