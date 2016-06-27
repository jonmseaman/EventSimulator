using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventSimulator.Events;
using Microsoft.ServiceBus;
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
        private static int[] behaviorPercents;
        private static int BatchSize { get; set; } = 512;

        static void Main(string[] args)
        {
            // Get distribution of events from the command line parameters.
            behaviorPercents = new int[3];
            try
            {
                var sum = 0;
                for (var i = 0; i < 3; i++)
                {
                    behaviorPercents[i] = int.Parse(args[i]);
                    sum += behaviorPercents[i];
                }

                if (sum != 100)
                {
                    throw new ArgumentException("Command line arguments do not add up to 100%.");
                }

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.WriteLine("Usage:\n" +
                                  System.AppDomain.CurrentDomain.FriendlyName +
                                  " %UsersPurchasingFirstProductViewed %UsersPurchasingAfterBrowsing %UsersBrowsing");
                Console.WriteLine("These must add up to 100%.");
                Console.ResetColor();
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
                Environment.Exit(exitCode: 1);
            }


            // Get data from config file.
            var config = ConfigurationManager.AppSettings;
            // Get the conenction string from the config file.
            string connectionString = config["ConnectionString"];
            if (connectionString == null)
            { // TODO: What kind of exception should be thrown?
                throw new Exception("Could not find 'ConnectionString' in AppSettings in App.Config");
            }
            // Get batch size from config file.
            var batchSizeStr = config["BatchSize"];
            if (batchSizeStr == null)
            {
                Console.WriteLine($"Couldn't find batch size in App.Config. Using default {BatchSize}.");
            }
            int batchSize;
            if (!int.TryParse(batchSizeStr, out batchSize))
            {
                Console.WriteLine($"Couldn't parse BatchSize from App.Config. Using default {BatchSize}.");
            }

            // Set up threads.
            var numThreads = Environment.ProcessorCount;
            var eventsSentByThread = new int[numThreads];
            var threads = new Thread[numThreads];
            var timeStarted = DateTime.Now;
            for (var i = 0; i < numThreads; i++)
            {
                var i1 = i;
                threads[i] = new Thread(() => SendEvents(connectionString, ref eventsSentByThread[i1]));
                threads[i].Start();
            }


            // Thread to show the user how many events are being sent.
            var countThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    // Count events sent.
                    var sum = eventsSentByThread.Sum();
                    var eps = sum/(DateTime.Now - timeStarted).TotalSeconds;
                    Console.Write("Events sent: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(sum.ToString());
                    Console.ResetColor();
                    Console.Write(". Events per second: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(eps);
                    Console.ResetColor();
                }
            });
            countThread.Start();


            // Done.
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
            for (var i = 0; i < numThreads; i++)
            {
                threads[i].Abort();
            }
        }

        public static void SendEvents(string connectionString, ref int eventsSent)
        {
            // Event sender and receiver.
            var eventSender = new Sender(connectionString);

            // Create list of events to send to eventHub
            var eventList = new List<Event>();
            for (var i = 0; i < BatchSize; i++)
            {
                eventList.Add(EventCreator.CreateClickEvent());
            }

            while (true)
            {
                // Update list of events to show user action.
                var offset = 0;
                var fastPurchaseCount = behaviorPercents[0] * BatchSize / 100;
                var slowPurchaseCount = behaviorPercents[1] * BatchSize / 100;
                var browsingCount = eventList.Count - fastPurchaseCount - slowPurchaseCount;
                
                UpdateEvents(offset, fastPurchaseCount, eventList, UserBehavior.FastPurchase);
                offset += fastPurchaseCount;
                UpdateEvents(offset, slowPurchaseCount, eventList, UserBehavior.SlowPurchase);
                offset += slowPurchaseCount;
                UpdateEvents(offset, browsingCount, eventList, UserBehavior.Browsing);


                // Send the events
                try
                {
                    eventSender.SendBatch(eventList);
                    eventsSent += eventList.Count;
                }
                catch (MessageSizeExceededException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Batch size exceeded.");
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                    break;
                }
            }
        }

        private static void UpdateEvents(int startIndex, int count, List<Event> eventList , UserBehavior behavior)
        {
            for (var i = startIndex; count > 0; count--, i++)
            {
                eventList[i] = EventCreator.CreateNextEvent(eventList[i], behavior);
            }
        }
    }
}
