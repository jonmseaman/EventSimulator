using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace EventSimulator
{
    /// <summary>
    /// This program is supposed to simulate usage of an ecommerce website.
    /// This program sends events corresponding to navigating around the website
    /// as well as making purchases. The events are sent to an event hub corresponding
    /// to a connection string stored in the App.config file.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Get data from config file and command line parameters.
            var config = ConfigurationManager.AppSettings;
            // Get the conenction string from the config file.
            string connectionString = config["ConnectionString"];
            if (connectionString == null)
            { // TODO: Throw exception or just print error?
                throw new NullReferenceException("Could not find 'ConnectionString' in AppSettings");
            }

            Thread eventThread = new Thread(async () =>
            {
                while (true)
                {
                    // Event sender and receiver.
                    var eventCreator = new EventCreator();
                    var eventSender = new Sender(connectionString);

                    // Create the events
                    var eventList = eventCreator.CreateUserEventSequence();

                    // Send the events
                    Console.WriteLine("Sending the results.");
                    await eventSender.SendBatchAsync(eventList);
                    Console.WriteLine("Finished Sending data.");
                }
            });
            eventThread.Start();

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
            eventThread.Abort();
        }
    }
}
