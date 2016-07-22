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

namespace EventSimulator.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EventHubControl : UserControl
    {
        public Settings Settings = new Settings();
        private Simulator.Simulator _simulator;

        public EventHubControl()
        {
            InitializeComponent();
        }

        private bool _started = false;
        private void StartStopButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_started) return;
            _started = true;
            Settings.ConnectionString =
                "Endpoint=sb://servicebusintern2016.servicebus.windows.net/;SharedAccessKeyName=EventHubSendKey;SharedAccessKey=k5tXsmofhbULo+odj+QmCR8yM6oR0pOPNV1/OP5lhxw=;EntityPath=sellersite";
            _simulator = new Simulator.Simulator(Settings);
            _simulator.StartSending();
        }
    }
}
