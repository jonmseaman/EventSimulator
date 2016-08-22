namespace EventSimulator.Simulator
{
    using Events;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;
    using SellerSite;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Class that manages the creation and sending of events.
    /// Uses <see cref="EventCreator"/> for event creation.
    /// </summary>
    public class Simulator : INotifyPropertyChanged
    {
        #region Public Variables

        private int _eventsSent;

        /// <summary>
        /// The number of events sent by the simulator to the
        /// event hub.
        /// </summary>
        public int EventsSent
        {
            get { return _eventsSent; }
            private set
            {
                if (value == _eventsSent) return;
                _eventsSent = value;
                NotifyPropertyChanged();
            }
        }

        private double _eventsPerSecond;

        /// <summary>
        /// The number of events sent each second.
        /// Calculated and updated manually in this class.
        /// </summary>
        public double EventsPerSecond
        {
            get { return _eventsPerSecond; }
            set
            {
                if (!(Math.Abs(value - _eventsPerSecond) >= float.Epsilon)) return;
                _eventsPerSecond = value;
                NotifyPropertyChanged();
            }
        }


        private SimulatorStatus _simulatorStatus = SimulatorStatus.Stopped;

        /// <summary>
        /// Used so that the GUI can know if the <see cref="Simulator"/> is
        /// sending, shutting down, or stopped.
        /// </summary>
        public SimulatorStatus Status
        {
            get { return _simulatorStatus; }
            private set
            {
                if (value == _simulatorStatus) return;
                _simulatorStatus = value;
                NotifyPropertyChanged();
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
        private CreateListDelegate _createList;

        private delegate void UpdateListDelegate(List<Event> events);

        /// <summary>
        /// The method that is used to create the next batch of events from
        /// an existing batch.
        /// </summary>
        private UpdateListDelegate _updateList;

        /// <summary>
        /// Contains settings that tell the simulator what event hub to send
        /// to, how many, what type, etc.
        /// </summary>
        private readonly Settings _settings;

        /// <summary>
        /// This is the client that is used to create event senders.
        /// </summary>
        private EventHubClient _eventHubClient;

        private readonly EventCreator _eventCreator = new EventCreator();

        private ConcurrentQueue<List<EventData>> _dataQueue = new ConcurrentQueue<List<EventData>>();

        // Threads
        private Thread[] _senderThreads;
        private Thread _creationThread;
        private Thread _eventsCountThread;

        // Event sent by threads
        private int[] _eventsSentPerThread;

        #endregion

        #region Constructor

        public Simulator(Settings s)
        {
            _settings = s;
        }

        #endregion

        #region Sending / Stop Sending

        /// <summary>
        /// Start sending events to event hub.
        /// </summary>
        public void StartSending()
        {
            if (Status == SimulatorStatus.Sending) return;
            Status = SimulatorStatus.Sending;
            SetupDelegates(_settings.SendMode);

            _eventHubClient = EventHubClient.CreateFromConnectionString(_settings.ConnectionString);

            // Setup threads
            StartSending(_settings.ThreadsCount, _dataQueue);
        }

        private void StartSending(int numSenders, ConcurrentQueue<List<EventData>> queue)
        {
            // Initialize variables
            _dataQueue = new ConcurrentQueue<List<EventData>>();

            // Creation thread
            // Set up creation threads
            _creationThread = new Thread(() =>
            {
                CreateEvents(queue);
            });
            _creationThread.Start();


            // Sender threads
            _senderThreads = new Thread[numSenders];
            _eventsSentPerThread = new int[numSenders];
            for (var i = 0; i < numSenders; i++)
            {
                _eventsSentPerThread[i] = 0;
                var i1 = i; // Make sure the lambda gets the right value of i.
                _senderThreads[i] = new Thread(() => SendEvents($"eventSimulator{i1}", ref _eventsSentPerThread[i1], queue));
                _senderThreads[i].Start();
            }

            // Events Per second thread
            _eventsCountThread = new Thread(() =>
            {
                var next = DateTime.Now;
                var dt = TimeSpan.FromSeconds(1);
                var prevEventsSent = 0;
                while (Status == SimulatorStatus.Sending)
                {
                    next += dt;
                    EventsSent = _eventsSentPerThread.Sum();
                    EventsPerSecond = (EventsSent - prevEventsSent) / dt.TotalSeconds;
                    prevEventsSent = EventsSent;
                    var now = DateTime.Now;
                    if (next > now)
                    {
                        Thread.Sleep(next - now);
                    }
                }
            });
            _eventsCountThread.Start();
        }

        /// <summary>
        /// Stop sending events to the event hub.
        /// </summary>
        public void StopSending()
        {
            Status = SimulatorStatus.Stopping;
            _creationThread.Join();
            foreach (var t in _senderThreads)
            {
                t.Join();
            }

            _eventsCountThread.Join();
            Status = SimulatorStatus.Stopped;
        }

        #endregion

        #region Private methods

        #region Setup

        private void SetupDelegates(SendMode sendMode)
        {
            if (sendMode == SendMode.ClickEvents)
            {
                _createList = CreateClickEvents;
                _updateList = UpdateClickEvents;
            }
            else if (sendMode == SendMode.PurchaseEvents)
            {
                _createList = CreatePurchaseEvents;
                _updateList = UpdatePurchaseEvents;
            }
            else if (sendMode == SendMode.SimulatedEvents)
            {
                _createList = CreateClickEvents;
                _updateList = UpdateSimulatedEvents;
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
            var sender = _eventHubClient.CreateSender(publisher);

            while (Status == SimulatorStatus.Sending)
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

            if (_settings.EventsPerSecond < _settings.BatchSize)
            {
                eventList = _createList(_settings.EventsPerSecond);
                dt = TimeSpan.FromSeconds(1.0);
            }
            else
            {
                // Make batch at max size
                eventList = _createList(_settings.BatchSize);

                // Calculate dt
                dt = TimeSpan.FromSeconds(((double)_settings.BatchSize) / _settings.EventsPerSecond);
            }

            var next = DateTime.Now;
            while (Status == SimulatorStatus.Sending)
            {
                // If we cannot send fast enough, don't keep making more events.
                if (batchQueue.Count >= 2 * _settings.ThreadsCount)
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
                _updateList(eventList);

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
                eventList.Add(_eventCreator.CreateClickEvent());
            }

            return eventList;
        }

        private List<Event> CreatePurchaseEvents(int batchSize)
        {
            var eventList = new List<Event>();
            for (var i = 0; i < batchSize; i++)
            {
                eventList.Add(_eventCreator.CreatePurchaseEvent());
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
            var fastPurchaseCount = _settings.BehaviorPercents[0] * len / 100;
            var slowPurchaseCount = _settings.BehaviorPercents[1] * len / 100;
            var browsingCount = len - fastPurchaseCount - slowPurchaseCount;

            UpdateSimulatedEventsWithBehavior(offset, fastPurchaseCount, eventList, UserBehavior.FastPurchase);
            offset += fastPurchaseCount;
            UpdateSimulatedEventsWithBehavior(offset, slowPurchaseCount, eventList, UserBehavior.SlowPurchase);
            offset += slowPurchaseCount;
            UpdateSimulatedEventsWithBehavior(offset, browsingCount, eventList, UserBehavior.Browsing);

        }

        private void UpdateSimulatedEventsWithBehavior(int startIndex, int count, List<Event> eventList, UserBehavior behavior)
        {
            for (var i = startIndex; count > 0; count--, i++)
            {
                eventList[i] = _eventCreator.CreateNextEvent(eventList[i], behavior);
            }
        }

        private void UpdateClickEvents(List<Event> eventList)
        {
            for (var i = 0; i < eventList.Count; i++)
            {
                eventList[i] = _eventCreator.CreateClickEvent();
            }

        }

        private void UpdatePurchaseEvents(List<Event> eventList)
        {
            for (var i = 0; i < eventList.Count; i++)
            {
                eventList[i] = _eventCreator.CreatePurchaseEvent();
            }
        }

        #endregion

        #endregion

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
