
namespace EventSimulator.Simulator
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using Events;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;
    using SellerSite;

    /// <summary>
    /// Class that manages the creation and sending of events.
    /// Uses <see cref="EventCreator"/> for event creation.
    /// </summary>
    public class Simulator : INotifyPropertyChanged
    {
        #region Public Variables

        private int eventsSent;

        /// <summary>
        /// The number of events sent by the simulator to the
        /// event hub.
        /// </summary>
        public int EventsSent
        {
            get { return this.eventsSent; }
            private set
            {
                if (value == this.eventsSent) return;
                this.eventsSent = value;
                this.NotifyPropertyChanged();
            }
        }

        private double eventsPerSecond;

        /// <summary>
        /// The number of events sent each second.
        /// Calculated and updated manually in this class.
        /// </summary>
        public double EventsPerSecond
        {
            get { return this.eventsPerSecond; }
            set
            {
                if (!(Math.Abs(value - this.eventsPerSecond) >= float.Epsilon)) return;
                this.eventsPerSecond = value;
                this.NotifyPropertyChanged();
            }
        }


        private SimulatorStatus simulatorStatus = SimulatorStatus.Stopped;

        /// <summary>
        /// Used so that the GUI can know if the <see cref="Simulator"/> is
        /// sending, shutting down, or stopped.
        /// </summary>
        public SimulatorStatus Status
        {
            get { return this.simulatorStatus; }
            private set
            {
                if (value == this.simulatorStatus) return;
                this.simulatorStatus = value;
                this.NotifyPropertyChanged();
            }

        }

        #endregion

        #region Private Variables

        /// <summary>
        /// Creates a list of events of length size.
        /// </summary>
        /// <param name="size">The length of the list to create.</param>
        /// <returns>A list of created events.</returns>
        private delegate List<Event> CreateListDelegate(int size);

        /// <summary>
        /// The method that is used to create a single batch of events.
        /// </summary>
        private CreateListDelegate createList;

        private delegate void UpdateListDelegate(List<Event> events);

        /// <summary>
        /// The method that is used to create the next batch of events from
        /// an existing batch.
        /// </summary>
        private UpdateListDelegate updateList;

        /// <summary>
        /// Contains settings that tell the simulator what event hub to send
        /// to, how many, what type, etc.
        /// </summary>
        private readonly Settings settings;

        /// <summary>
        /// This is the client that is used to create event senders.
        /// </summary>
        private EventHubClient eventHubClient;

        private readonly EventCreator eventCreator = new EventCreator();

        private ConcurrentQueue<List<EventData>> dataQueue = new ConcurrentQueue<List<EventData>>();

        // Threads
        private Thread[] senderThreads;
        private Thread creationThread;
        private Thread eventsCountThread;

        // Event sent by threads
        private int[] eventsSentPerThread;

        #endregion

        #region Constructor

        public Simulator(Settings s)
        {
            this.settings = s;
        }

        #endregion

        #region Sending / Stop Sending

        public void StartSending()
        {
            if (this.Status == SimulatorStatus.Sending) return;
            this.Status = SimulatorStatus.Sending;
            this.SetupDelegates(this.settings.SendMode);

            this.eventHubClient = EventHubClient.CreateFromConnectionString(this.settings.ConnectionString);

            // Setup threads
            this.StartSending(this.settings.ThreadsCount, this.dataQueue);
        }

        private void StartSending(int numSenders, ConcurrentQueue<List<EventData>> queue)
        {
            // Initialize variables
            this.dataQueue = new ConcurrentQueue<List<EventData>>();

            // Creation thread
            // Set up creation threads
            this.creationThread = new Thread(() =>
            {
                this.CreateEvents(queue);
            });
            this.creationThread.Start();


            // Sender threads
            this.senderThreads = new Thread[numSenders];
            this.eventsSentPerThread = new int[numSenders];
            for (var i = 0; i < numSenders; i++)
            {
                this.eventsSentPerThread[i] = 0;
                var i1 = i; // Make sure the lambda gets the right value of i.
                this.senderThreads[i] = new Thread(() => this.SendEvents($"eventSimulator{i1}", ref this.eventsSentPerThread[i1], queue));
                this.senderThreads[i].Start();
            }

            // Events Per second thread
            this.eventsCountThread = new Thread(() =>
            {
                var next = DateTime.Now;
                var dt = TimeSpan.FromSeconds(1);
                var prevEventsSent = 0;
                while (this.Status == SimulatorStatus.Sending)
                {
                    next += dt;
                    this.EventsSent = this.eventsSentPerThread.Sum();
                    this.EventsPerSecond = (this.EventsSent - prevEventsSent) / dt.TotalSeconds;
                    prevEventsSent = this.EventsSent;
                    var now = DateTime.Now;
                    if (next > now)
                    {
                        Thread.Sleep(next - now);
                    }
                }
            });
            this.eventsCountThread.Start();
        }

        public void StopSending()
        {
            this.Status = SimulatorStatus.Stopping;
            this.creationThread.Join();
            foreach (var t in this.senderThreads)
            {
                t.Join();
            }

            this.eventsCountThread.Join();
            this.Status = SimulatorStatus.Stopped;
        }

        #endregion

        #region Private methods

        #region Setup

        private void SetupDelegates(SendMode sendMode)
        {
            if (sendMode == SendMode.ClickEvents)
            {
                this.createList = this.CreateClickEvents;
                this.updateList = this.UpdateClickEvents;
            }
            else if (sendMode == SendMode.PurchaseEvents)
            {
                this.createList = this.CreatePurchaseEvents;
                this.updateList = this.UpdatePurchaseEvents;
            }
            else if (sendMode == SendMode.SimulatedEvents)
            {
                this.createList = this.CreateClickEvents;
                this.updateList = this.UpdateSimulatedEvents;
            }
        }

        #endregion

        #region SendEvents

        /// <summary>
        /// Takes batches from <see cref="batchQueue"/> and sends them to the event hub.
        /// </summary>
        /// <param name="publisher">A name that identifies the sender to the event hub.</param>
        /// <param name="count">When this method sends events, the number of events 
        /// sent are added to <see cref="count"/>.</param>
        /// <param name="batchQueue">The queue that batches are taken from before they are sent.</param>
        public void SendEvents(string publisher, ref int count, ConcurrentQueue<List<EventData>> batchQueue)
        {
            var sender = this.eventHubClient.CreateSender(publisher);

            while (this.Status == SimulatorStatus.Sending)
            {
                List<EventData> eventList;
                if (!batchQueue.TryDequeue(out eventList))
                {
                    Thread.Sleep(50);
                    continue;
                }

                // Send the events
                try
                {
                    sender.SendBatch(eventList);
                    count += eventList.Count;
                }
                catch (MessageSizeExceededException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Batch size exceeded.");
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                    break;
                }
                catch (ServerBusyException)
                {
                    Thread.Sleep(4000);
                }
                catch (ServerTooBusyException)
                {
                    Thread.Sleep(50);
                }
            }
        }

        #endregion

        #region CreateEvents

        /// <summary>
        /// Creates batches of simulated events, converts them to <see cref="EventData"/>
        /// then adds them to <see cref="batchQueue"/>.
        /// </summary>
        /// <param name="batchQueue">A Queue of batches of <see cref="EventData"/>.</param>
        public void CreateEvents(ConcurrentQueue<List<EventData>> batchQueue)
        {
            TimeSpan dt;
            List<Event> eventList;

            if (this.settings.EventsPerSecond < this.settings.BatchSize)
            {
                eventList = this.createList(this.settings.EventsPerSecond);
                dt = TimeSpan.FromSeconds(1.0);
            }
            else
            {
                // Make batch at max size
                eventList = this.createList(this.settings.BatchSize);

                // Calculate dt
                dt = TimeSpan.FromSeconds(((double)this.settings.BatchSize) / this.settings.EventsPerSecond);
            }

            var next = DateTime.Now;
            while (this.Status == SimulatorStatus.Sending)
            {
                // If we cannot send fast enough, don't keep making more events.
                if (batchQueue.Count >= 2 * this.settings.ThreadsCount)
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

                batchQueue.Enqueue(dataList);

                // Update
                this.updateList(eventList);

                var now = DateTime.Now;
                if (next > now)
                {
                    Thread.Sleep((int)(next - now).TotalMilliseconds);
                }

                next += dt;
            }
        }

        private List<Event> CreateClickEvents(int batchSize)
        {
            var eventList = new List<Event>();
            for (var i = 0; i < batchSize; i++)
            {
                eventList.Add(this.eventCreator.CreateClickEvent());
            }

            return eventList;
        }

        private List<Event> CreatePurchaseEvents(int batchSize)
        {
            var eventList = new List<Event>();
            for (var i = 0; i < batchSize; i++)
            {
                eventList.Add(this.eventCreator.CreatePurchaseEvent());
            }

            return eventList;
        }

        #endregion

        #region UpdateEvents

        private void UpdateSimulatedEvents(List<Event> eventList)
        {
            // Update list of events to show user action.
            var offset = 0;
            var len = eventList.Count;
            var fastPurchaseCount = this.settings.BehaviorPercents[0] * len / 100;
            var slowPurchaseCount = this.settings.BehaviorPercents[1] * len / 100;
            var browsingCount = len - fastPurchaseCount - slowPurchaseCount;

            this.UpdateSimulatedEventsWithBehavior(offset, fastPurchaseCount, eventList, UserBehavior.FastPurchase);
            offset += fastPurchaseCount;
            this.UpdateSimulatedEventsWithBehavior(offset, slowPurchaseCount, eventList, UserBehavior.SlowPurchase);
            offset += slowPurchaseCount;
            this.UpdateSimulatedEventsWithBehavior(offset, browsingCount, eventList, UserBehavior.Browsing);

        }

        private void UpdateSimulatedEventsWithBehavior(int startIndex, int count, List<Event> eventList, UserBehavior behavior)
        {
            for (var i = startIndex; count > 0; count--, i++)
            {
                eventList[i] = this.eventCreator.CreateNextEvent(eventList[i], behavior);
            }
        }

        private void UpdateClickEvents(List<Event> eventList)
        {
            for (var i = 0; i < eventList.Count; i++)
            {
                eventList[i] = this.eventCreator.CreateClickEvent();
            }

        }

        private void UpdatePurchaseEvents(List<Event> eventList)
        {
            for (var i = 0; i < eventList.Count; i++)
            {
                eventList[i] = this.eventCreator.CreatePurchaseEvent();
            }
        }

        #endregion

        #endregion

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion
    }

    public enum SimulatorStatus
    {
        Stopped, 
        Stopping, 
        Sending, 
    }
}
