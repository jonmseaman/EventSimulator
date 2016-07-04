using System.Configuration;
using System.Linq;

namespace EventSimulator
{
    internal class Settings
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
            // MaxThreads
            var maxThreadsStr = config["MaxThreads"];
            int maxThreads;
            if (maxThreadsStr != null && int.TryParse(maxThreadsStr, out maxThreads))
            {
                MaxThreads = maxThreads;
            }
            else
            {
                throw new ConfigurationErrorsException("MaxThreads not found in App.config.");
            }

        }

        #region Settings

        public int BatchSize { get; set; } = 512;
        public string ConnectionString { get; set; }

        /// <summary>
        ///     [0] - FastPurchase
        ///     [1] - SlowPurchase
        ///     [2] - Browsing
        /// </summary>
        public int[] BehaviorPercents { get; set; }

        public int EventsPerSecond { get; set; }

        public int MaxThreads { get; set; }

        #endregion
    }
}