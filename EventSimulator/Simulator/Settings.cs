
namespace EventSimulator.Simulator
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class Settings
    {
        #region Settings

        public int BatchSize { get; set; } = 512;
        public string ConnectionString { get; set; }

        /// <summary>
        ///     [0] - FastPurchase
        ///     [1] - SlowPurchase
        ///     [2] - Browsing
        /// </summary>
        public int[] BehaviorPercents { get; set; } = { 15, 30, 55 };

        public int EventsPerSecond { get; set; } = 10;

        public int ThreadsCount { get; set; } = Environment.ProcessorCount;

        public SendMode SendMode { get; set; } = SendMode.SimulatedEvents;

        #endregion

        #region Load, Save

        private const string SaveDir = "Data/Settings/";

        public static async Task<Settings> Load(string jsonFileName)
        {
            var fName = Path.GetFileNameWithoutExtension(jsonFileName);
            Debug.Assert(fName != null && fName.Equals(jsonFileName));
            var filePath = $"{SaveDir}{jsonFileName}.json";

            var objectString = string.Empty;
            using (StreamReader sr = File.OpenText(filePath))
            {
                objectString = await sr.ReadToEndAsync();

            }

            return JsonConvert.DeserializeObject<Settings>(objectString);
        }

        public static void Save(Settings s, string jsonFileName)
        {
            var fName = Path.GetFileNameWithoutExtension(jsonFileName);
            Debug.Assert(fName != null && fName.Equals(jsonFileName));
            var filePath = $"{SaveDir}{jsonFileName}.json";

            // Make sure that the directory exists.
            Directory.CreateDirectory(filePath);

            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine(JsonConvert.SerializeObject(s));
            }
        }

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