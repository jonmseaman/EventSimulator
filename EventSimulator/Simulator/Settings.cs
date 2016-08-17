
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

        /// <summary>
        /// The number of events to send in each batch.
        /// </summary>
        public int BatchSize { get; set; } = 512;

        /// <summary>
        /// The connection string to the event hub.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     [0] - FastPurchase
        ///     [1] - SlowPurchase
        ///     [2] - Browsing
        /// </summary>
        public int[] BehaviorPercents { get; set; } = { 15, 30, 55 };

        /// <summary>
        /// The desired number of events to send each second.
        /// </summary>
        public int EventsPerSecond { get; set; } = 10;

        /// <summary>
        /// The number of threads to use for sending events.
        /// </summary>
        public int ThreadsCount { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Determines what types of events are going to be sent.
        /// See README.md for more information.
        /// </summary>
        public SendMode SendMode { get; set; } = SendMode.SimulatedEvents;

        #endregion

        #region Load, Save

        /// <summary>
        /// Determines directory where settings will be saved and loaded from.
        /// </summary>
        private const string SaveDir = "Data/Settings/";

        /// <summary>
        /// Loads settings from <see cref="SaveDir"/>.
        /// </summary>
        /// <param name="jsonFileName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Saves settings to <see cref="SaveDir"/>
        /// </summary>
        /// <param name="s">The settings object that will be saved.</param>
        /// <param name="jsonFileName">The file name without extension.</param>
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