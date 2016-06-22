using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace EventSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get data from config file and command line parameters.
            // TODO: Get command line parameters.
            var config = ConfigurationManager.AppSettings;
            // Get the conenction string from the config file.
            string connectionString = config["ConnectionString"];
            if (connectionString == null)
            { // TODO: Throw exception or just print error?
                throw new NullReferenceException("Could not find 'ConnectionString' in AppSettings");
            }
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString);

            // Make an event creator.
            // Make an event sender.
            // While the user has not pressed enter:
            //// Create events with the event creator.
            //// Send a batch of events. (on a separate thread.)
            Console.ReadLine();
        }
    }
}
