using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventSimulator.Events;
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
            { // TODO: What kind of exception should be thrown?
                throw new Exception("Could not find 'ConnectionString' in AppSettings in App.Config");
            }

            Thread eventThread = new Thread(async () =>
            {
                // Event sender and receiver.
                var eventCreator = new EventCreator();
                var eventSender = new Sender(connectionString);

                // Create list of events to send to eventHub
                var eventList = new List<Event>();
                for (var i = 0; i < 256; i++)
                {
                    eventList.Add(eventCreator.CreateClickEvent());
                }

                while (true)
                {
                    // Update list of events to show user action.
                    for (var i = 0; i < eventList.Count; i++)
                    {
                        eventList[i] = eventCreator.CreateNextEvent(eventList[i], UserBehavior.FastPurchase);
                    }                    

                    // Send the events
                    Console.WriteLine("Sending the results.");
                    try
                    {
                        await eventSender.SendBatchAsync(eventList);
                    }
                    catch (MessageSizeExceededException e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Batch size exceeded. ");
                        Console.WriteLine(e.Message);
                        Console.ResetColor();
                        while (eventList.Count > 0)
                        {
                            eventSender.Send(eventList[0]);
                            eventList.RemoveAt(0);
                        }
                    }
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
