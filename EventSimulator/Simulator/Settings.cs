using System;
using System.Configuration;
using System.Linq;

namespace EventSimulator.Simulator
{
    public class Settings
    {
        /// <summary>
        ///     Loads settings from App.Config.
        /// </summary>
        public void Load()
        {
            // Make config mngr
            // Get data from config file.
            var config = ConfigurationManager.AppSettings;
            // Get the connection string
            var connectionString = config["ConnectionString"];
            if (connectionString != null)
            {
                ConnectionString = connectionString;
            }
            // Get the batch size.
            var batchSizeStr = config["BatchSize"];
            var sz = 0;
            if (batchSizeStr != null && int.TryParse(batchSizeStr, out sz))
            {
                BatchSize = sz;
            }
            else
            {
                throw new ConfigurationErrorsException("Failed to load BatchSize from App.config.");
            }
            // Get the behavior percents.
            BehaviorPercents = new int[3];
            var percentStrs = new string[3];
            percentStrs[0] = config["FastPurchasePercent"];
            percentStrs[1] = config["SlowPurchasePercent"];
            percentStrs[2] = config["BrowsingPercent"];
            if (percentStrs.Contains(null))
            {
                throw new ConfigurationErrorsException("Not all behavior percents found in App.config.");
            }
            for (var i = 0; i < 3; i++)
            {
                BehaviorPercents[i] = int.Parse(percentStrs[i]);
            }

            if (BehaviorPercents.Sum() != 100)
            {
                throw new ConfigurationErrorsException("Behavior percents in App.config don't add up to 100.");
            }
            // Get events per second
            var eventsPerSecond = config["EventsPerSecond"];
            if (eventsPerSecond == null)
            {
                throw new ConfigurationErrorsException("EventsPerSecond not found in App.config.");
            }
            EventsPerSecond = int.Parse(eventsPerSecond);

            // ThreadsCount
            var maxThreadsStr = config["ThreadsCount"];
            int maxThreads;
            if (maxThreadsStr != null && int.TryParse(maxThreadsStr, out maxThreads))
            {
                ThreadsCount = maxThreads;
            }
            else
            {
                throw new ConfigurationErrorsException("ThreadsCount not found in App.config.");
            }

            var isFirstRunStr = config["IsFirstRun"];
            bool isFirstRun;
            // Default to true if parse failure.
            if (!bool.TryParse(isFirstRunStr, out isFirstRun))
            {
                isFirstRun = true;
            }
            IsFirstRun = isFirstRun;

            // Send mode
            var sendModeStr = config["SendMode"];
            SendMode sendMode;
            Enum.TryParse(sendModeStr, out sendMode);
            SendMode = sendMode;

        }

        public void Save()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var app = config.AppSettings.Settings;

            // Add settings
            app.Clear(); // Necessary to keep from writing the setting twice.
            app.Add("BatchSize", BatchSize.ToString());
            app.Add("ConnectionString", ConnectionString);
            app.Add("EventsPerSecond", EventsPerSecond.ToString());
            app.Add("FastPurchasePercent", BehaviorPercents[0].ToString());
            app.Add("SlowPurchasePercent", BehaviorPercents[1].ToString());
            app.Add("BrowsingPercent", BehaviorPercents[2].ToString());
            app.Add("ThreadsCount", ThreadsCount.ToString());
            app.Add("IsFirstRun", IsFirstRun.ToString());
            app.Add("SendMode", SendMode.ToString());
            config.Save(ConfigurationSaveMode.Modified);
        }

        #region Settings

        public int BatchSize { get; set; } = 512;
        public string ConnectionString { get; set; }

        /// <summary>
        ///     [0] - FastPurchase
        ///     [1] - SlowPurchase
        ///     [2] - Browsing
        /// </summary>
        public int[] BehaviorPercents { get; set; } = { 15, 30, 55 };

        public int EventsPerSecond { get; set; } = 1;

        public int ThreadsCount { get; set; } = Environment.ProcessorCount;

        public bool IsFirstRun { get; set; } = true;

        public SendMode SendMode { get; set; } = SendMode.SimulatedEvents;

        #endregion



    }

    /// <summary>
    /// Used to specify what type of events the program will be sending.
    /// </summary>
    public enum SendMode
    {
        /// <summary>
        /// Just send click events.
        /// </summary>
        ClickEvents,
        /// <summary>
        /// Just send Purchase events
        /// </summary>
        PurchaseEvents,
        /// <summary>
        /// Send simulated purchase events and click events.
        /// </summary>
        SimulatedEvents
    }
}