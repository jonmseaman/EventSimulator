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
    /// This program sends simulated events to an event hub according 
    /// to its App.config and cmd line arguments.
    /// </summary>
    static class Program
    {
        #region Member Variables

        private static int[] behaviorPercents;
        private static int BatchSize { get; set; } = 512;

        delegate List<Event> CreateListDelegate();

        private static CreateListDelegate CreateList;

        delegate void UpdateListDelegate(List<Event> events);

        static UpdateListDelegate UpdateList;

        #endregion

        static void Main(string[] args)
        {
            // Load settings
            try
            {
                Settings.Load();
                behaviorPercents = Settings.BehaviorPercents;
                BatchSize = Settings.BatchSize;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
                Environment.Exit(exitCode: 1);
            }

            // TODO:
            // Set up delegates
            if (args.Length > 0 && args[0].Equals("ClickEvents"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Sending click events.");
                Console.ResetColor();
                CreateList = CreateClickEvents;
                UpdateList = UpdateClickEvents;
            } else if (args.Length > 0 && args[0].Equals("PurchaseEvents"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Sending purchase events.");
                Console.ResetColor();
                CreateList = CreatePurchaseEvents;
                UpdateList = UpdatePurchaseEvents;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Sending simulated events.");
                Console.ResetColor();
                CreateList = CreateClickEvents;
                UpdateList = UpdateSimulatedEvents;
            }

            // Set up threads.
            var numThreads = Settings.MaxThreads;
            if (numThreads == 0)
            {
                numThreads = Environment.ProcessorCount;
            }
            var eventsSentByThread = new int[numThreads];
            var threads = new Thread[numThreads];
            var timeStarted = DateTime.Now;
            for (var i = 0; i < numThreads; i++)
            {
                var i1 = i; // Make sure the lambda gets the right value of i.
                threads[i] = new Thread(() => SendEvents(Settings.ConnectionString, ref eventsSentByThread[i1]));
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
                    Console.WriteLine($"Events sent: {sum}\tEvents per second: {eps}");
                }
            });
            countThread.Start();

            // Done.
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
            countThread.Abort();
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
            var eventList = CreateList();

            while (true)
            {
                UpdateList(eventList);

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

        #region CreateEvents

        private static List<Event> CreateClickEvents()
        {
            var eventList = new List<Event>();
            for (var i = 0; i < BatchSize; i++)
            {
                eventList.Add(EventCreator.CreateClickEvent());
            }
            return eventList;
        }

        private static List<Event> CreatePurchaseEvents()
        {
            var eventList = new List<Event>();
            for (var i = 0; i < BatchSize; i++)
            {
                eventList.Add(EventCreator.CreatePurchaseEvent());
            }
            return eventList;
        }

        #endregion

        #region UpdateEvents

        private static void UpdateSimulatedEvents(List<Event> eventList)
        {
            // Update list of events to show user action.
            var offset = 0;
            var fastPurchaseCount = behaviorPercents[0] * BatchSize / 100;
            var slowPurchaseCount = behaviorPercents[1] * BatchSize / 100;
            var browsingCount = eventList.Count - fastPurchaseCount - slowPurchaseCount;

            UpdateSimulatedEventsWithBehavior(offset, fastPurchaseCount, eventList, UserBehavior.FastPurchase);
            offset += fastPurchaseCount;
            UpdateSimulatedEventsWithBehavior(offset, slowPurchaseCount, eventList, UserBehavior.SlowPurchase);
            offset += slowPurchaseCount;
            UpdateSimulatedEventsWithBehavior(offset, browsingCount, eventList, UserBehavior.Browsing);

        }

        private static void UpdateSimulatedEventsWithBehavior(int startIndex, int count, List<Event> eventList, UserBehavior behavior)
        {
            for (var i = startIndex; count > 0; count--, i++)
            {
                eventList[i] = EventCreator.CreateNextEvent(eventList[i], behavior);
            }
        }

        private static void UpdateClickEvents(List<Event> eventList)
        {
            for (var i = 0; i < eventList.Count; i++)
            {
                eventList[i] = EventCreator.CreateClickEvent();
            }

        }

        private static void UpdatePurchaseEvents(List<Event> eventList)
        {
            for (var i = 0; i < eventList.Count; i++)
            {
                eventList[i] = EventCreator.CreatePurchaseEvent();
            }
        }

        #endregion

    }
}
