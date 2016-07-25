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
using EventSimulator.Simulator;
using System.Threading;

namespace EventSimulator.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EventHubControl : UserControl
    {
        private Simulator.Simulator _simulator;

        private int _eventsPerSecond;
        public int EventsPerSecond
        {
            get { return _eventsPerSecond; }
            set {
                if (_eventsPerSecond != value)
                {
                    _eventsPerSecond = value;
                    TEventsPerSecond.Text = value.ToString();
                }
            }
        }

        private int _eventsSent;
        public int EventsSent
        {
            get { return _eventsSent; }
            set
            {
                if (_eventsSent != value)
                {
                    _eventsSent = value;
                    TEventsSent.Text = value.ToString();
                }
            }
        }

        public EventHubControl(Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("Settings cannot be null.");
            }

            InitializeComponent();
            _simulator = new Simulator.Simulator(settings);

            // Bind simulator status to GUI
            var myBinding = new Binding("Status") {Source = _simulator};
            TSimulatorStatus.SetBinding(TextBlock.TextProperty, myBinding);
        }

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
    }
}
