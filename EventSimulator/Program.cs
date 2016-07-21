using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EventSimulator.Events;
using Microsoft.ServiceBus.Messaging;
using System.Configuration;
using System.Deployment.Application;
using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;
using EventSimulator.SellerSite;

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

        delegate List<Event> CreateListDelegate(int size);
        static CreateListDelegate CreateList;

        delegate void UpdateListDelegate(List<Event> events);
        static UpdateListDelegate UpdateList;

        static Settings settings;

        private static EventHubClient EventHubClient;

        private static EventCreator eventCreator = new EventCreator();

        #endregion
        /*
        static void Main(string[] args)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Console.WriteLine($"Version: {ApplicationDeployment.CurrentDeployment.CurrentVersion}");
            }

            // The first part of this program decides whether to run setup or not.
            // The program recommends running the setup if the user has not run
            // the program before.

            // Get isFirstRun from config if there is a variable.
            settings = new Settings();
            settings.Load();

            // Default to running setup for the first run of the program.
            Console.Write($"Run setup? <{true}/{false}> ({settings.IsFirstRun}): ");
            var choiceStr = Console.ReadLine();
            bool runSetup;
            // Run setup if parse failure.
            if (!bool.TryParse(choiceStr, out runSetup))
            {
                runSetup = settings.IsFirstRun;
            }


            // If the previous section specifies running setup, settings are obtained from user.

            // Get or load settings.
            if (runSetup)
            {
                try
                {
                    GetSettingsFromUser(settings);
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
            }

            // Set up delegates
            SendMode sendMode;
            if (args.Length > 0 && Enum.TryParse(args[0], out sendMode))
            {
                settings.SendMode = sendMode;
            }

            if (settings.SendMode == SendMode.ClickEvents)
            {
                CreateList = CreateClickEvents;
                UpdateList = UpdateClickEvents;
            }
            else if (settings.SendMode == SendMode.PurchaseEvents)
            {
                CreateList = CreatePurchaseEvents;
                UpdateList = UpdatePurchaseEvents;
            }
            else
            {
                CreateList = CreateClickEvents;
                UpdateList = UpdateSimulatedEvents;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Sending {settings.SendMode:G}");
            Console.ResetColor();

            // Set up sending threads
            var queue = new ConcurrentQueue<List<EventData>>();
            EventHubClient = EventHubClient.CreateFromConnectionString(settings.ConnectionString);
            var numThreads = settings.ThreadsCount;
            if (numThreads == 0)
            {
                numThreads = Environment.ProcessorCount;
            }
            Console.WriteLine($"Making {numThreads} threads.");
            var eventsSentByThread = new int[numThreads];
            var threads = new Thread[numThreads];
            for (var i = 0; i < numThreads; i++)
            {
                var i1 = i; // Make sure the lambda gets the right value of i.
                threads[i] = new Thread(() => SendEvents(settings.ConnectionString, ref eventsSentByThread[i1], queue));
                threads[i].Start();
            }

            // Set up creation threads
            var createThread = new Thread(() =>
            {
                CreateEvents(queue);
            });
            createThread.Start();


            // Thread to show the user how many events are being sent.
            var countThread = new Thread(() =>
            {
                int previousSum = 0;
                var previousTime = DateTime.Now;
                while (true)
                {
                    Thread.Sleep(1000);
                    // Count events sent.
                    var sum = eventsSentByThread.Sum();
                    var time = DateTime.Now;
                    var eps = (sum - previousSum) / (DateTime.Now - previousTime).TotalSeconds;
                    previousSum = sum;
                    previousTime = time;
                    Console.WriteLine($"Events sent: {sum}, {eps:F0} per second.");
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
        */
        /// <summary>
        /// Allows the user to specify settings that would otherwise be loaded
        /// from a configuration file.
        /// </summary>
        /// <param name="userSettings">Puts settings entered into this settings object.</param>
        static void GetSettingsFromUser(Settings userSettings)
        {
            // Connection string
            Console.Write("Enter the connection string for the event hub: ");
            userSettings.ConnectionString = Console.ReadLine();

            // Send mode
            SendMode sendMode;
            string sendModeStr;
            Console.Write($"SendModes: {SendMode.ClickEvents:G}, {SendMode.PurchaseEvents:G}, {SendMode.SimulatedEvents:G}\n");
            do
            {
                Console.Write("Enter a SendMode: ");
                sendModeStr = Console.ReadLine();
            } while (!Enum.TryParse(sendModeStr, out sendMode));
            userSettings.SendMode = sendMode;

            // BehaviorPercents
            if (userSettings.SendMode == SendMode.SimulatedEvents)
            {
                var strs = new string[3];
                var percs = new int[3];
                Console.WriteLine("Enter user behaviors by percentage:");

                Console.Write("FastPurchase: ");
                strs[0] = Console.ReadLine();
                int.TryParse(strs[0], out percs[0]);

                Console.Write("SlowPurchase: ");
                strs[1] = Console.ReadLine();
                int.TryParse(strs[1], out percs[1]);

                Console.Write("Browsing: ");
                percs[2] = 100 - percs[0] - percs[1];
                Console.WriteLine(percs[2]);

                userSettings.BehaviorPercents = percs;

                if (percs.Sum() != 100)
                {
                    throw new ConfigurationErrorsException("Behavior percents must add up to 100%.");
                }
            }


            // Events per second
            Console.Write("Enter the number of events to send per second: ");
            int eventsPerSecond;
            int.TryParse(Console.ReadLine(), out eventsPerSecond);
            userSettings.EventsPerSecond = eventsPerSecond;


            // Max Threads
            Console.Write("Enter the number of threads to use: ");
            int maxThreads;
            int.TryParse(Console.ReadLine(), out maxThreads);
            userSettings.ThreadsCount = maxThreads;

            // Would you like to save these settings?
            Console.Write($"Save settings for next run <{true}/{false}>: ");
            var saveSettingsStr = Console.ReadLine();
            bool shouldSave;
            bool.TryParse(saveSettingsStr, out shouldSave);

            if (shouldSave)
            {
                Console.WriteLine("Saving...");
                userSettings.IsFirstRun = false;
                userSettings.Save();
                Console.WriteLine("Done.");
            }
            else
            {
                Console.WriteLine("Not saving.");
            }


        }

        public static void SendEvents(string connectionString, ref int eventsSent, ConcurrentQueue<List<EventData>> dataQueue)
        {
            List<EventData> eventList;


            while (true)
            {
                if (!dataQueue.TryDequeue(out eventList))
                {
                    Thread.Sleep(50);
                    continue;
                }
                // Send the events
                try
                {
                    EventHubClient.SendBatch(eventList);
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

        public static void CreateEvents(ConcurrentQueue<List<EventData>> dataQueue)
        {
            TimeSpan dt;
            List<Event> eventList;

            if (settings.EventsPerSecond < settings.BatchSize)
            {
                eventList = CreateList(settings.EventsPerSecond);
                dt = TimeSpan.FromSeconds(1.0);
            }
            else
            {
                // Make batch at max size
                eventList = CreateList(settings.BatchSize);
                // Calculate dt
                dt = TimeSpan.FromSeconds(((double)settings.BatchSize) / settings.EventsPerSecond);
            }

            var next = DateTime.Now;
            while (true)
            {
                // If we cannot send fast enough, don't keep making more events.
                if (dataQueue.Count >= 2*settings.ThreadsCount)
                {
                    Thread.Sleep(50);
                    continue;
                }

                // Make an EventData list to add to the queue
                var dataList = new List<EventData>(eventList.Count);
                foreach (var ev in eventList)
                {
                    var serializedString = JsonConvert.SerializeObject(ev);
                    dataList.Add(new EventData(Encoding.UTF8.GetBytes(serializedString)));
                }
                dataQueue.Enqueue(dataList);

                // Update
                UpdateList(eventList);

                var now = DateTime.Now;
                if (next > now)
                {
                    Thread.Sleep((int)(next - now).TotalMilliseconds);
                }
                next += dt;
            }
        }

        private static List<Event> CreateClickEvents(int batchSize)
        {
            var eventList = new List<Event>();
            for (var i = 0; i < batchSize; i++)
            {
                eventList.Add(eventCreator.CreateClickEvent());
            }
            return eventList;
        }

        private static List<Event> CreatePurchaseEvents(int batchSize)
        {
            var eventList = new List<Event>();
            for (var i = 0; i < batchSize; i++)
            {
                eventList.Add(eventCreator.CreatePurchaseEvent());
            }
            return eventList;
        }

        #endregion

        #region UpdateEvents

        private static void UpdateSimulatedEvents(List<Event> eventList)
        {
            // Update list of events to show user action.
            var offset = 0;
            var len = eventList.Count;
            var fastPurchaseCount = settings.BehaviorPercents[0] * len / 100;
            var slowPurchaseCount = settings.BehaviorPercents[1] * len / 100;
            var browsingCount = len - fastPurchaseCount - slowPurchaseCount;

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
                eventList[i] = eventCreator.CreateNextEvent(eventList[i], behavior);
            }
        }

        private static void UpdateClickEvents(List<Event> eventList)
        {
            for (var i = 0; i < eventList.Count; i++)
            {
                eventList[i] = eventCreator.CreateClickEvent();
            }

        }

        private static void UpdatePurchaseEvents(List<Event> eventList)
        {
            for (var i = 0; i < eventList.Count; i++)
            {
                eventList[i] = eventCreator.CreatePurchaseEvent();
            }
        }

        #endregion

    }
}
