using System.Configuration;

namespace Receiver
{
    internal class Settings
    {
        #region Settings

        public string ConnectionString { get; set; }

        public string EventHubName { get; set; }

        public string ConsumerGroup { get; set; } = "$Default";

        public bool IsFirstRun { get; set; } = true;

        public string StorageAccountName { get; set; }

        public string StorageAccountKey { get; set; }

        public string StorageConnectionString => $"DefaultEndpointsProtocol=https;AccountName={StorageAccountName};AccountKey={StorageAccountKey}";

        #endregion

        public void Load()
        {
            // Get data from the config file.
            var config = ConfigurationManager.AppSettings;

            // Get the connection string
            var connectionString = config["ConnectionString"];
            if (connectionString != null)
            {
                ConnectionString = connectionString;
            }

            // Get EventHubName
            var eventHubName = config["EventHubName"];
            if (eventHubName != null)
            {
                EventHubName = eventHubName;
            }

            // Get the consumer group.
            var consumerGroup = config["ConsumerGroup"];
            if (consumerGroup != null)
            {
                ConsumerGroup = consumerGroup;
            }

            // IsFirstRun
            var isFirstRunStr = config["IsFirstRun"];
            bool isFirstRun;
            if (bool.TryParse(isFirstRunStr, out isFirstRun))
            {
                IsFirstRun = isFirstRun;
            }

            // StorageAccountName
            var storageAccountName = config["StorageAccountName"];
            if (storageAccountName != null)
            {
                StorageAccountName = storageAccountName;
            }

            // StorageAccountKey
            var storageAccountKey = config["StorageAccountKey"];
            if (storageAccountKey != null)
            {
                StorageAccountKey = storageAccountKey;
            }

        }

        public void Save()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var app = config.AppSettings.Settings;

            // Add settings.
            app.Clear();
            app.Add("ConnectionString", ConnectionString);
            app.Add("EventHubName", EventHubName);
            app.Add("ConsumerGroup", ConsumerGroup);
            app.Add("IsFirstRun", IsFirstRun.ToString());
            app.Add("StorageAccountName", StorageAccountName);
            app.Add("StorageAccountKey", StorageAccountKey);
            config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
