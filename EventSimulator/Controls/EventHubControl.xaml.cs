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

namespace EventSimulator.Pages
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EventHubControl : UserControl
    {
        private Settings eventHubSettings = new Settings();

        public EventHubControl()
        {
            InitializeComponent();
        }

        private void StartStopButton_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
