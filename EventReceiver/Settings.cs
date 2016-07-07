using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Receiver
{
    internal class Settings
    {
        #region Settings
            
            public string ConnectionString { get; set; }

            public string ConsumerGroup { get; set; }

            public bool IsFirstRun { get; set; }

        #endregion

        public void Load() {
            // Get data from the config file.
            var config = ConfigurationManager.AppSettings;

            // Get the connection string
            var connectionString = config["ConnectionString"];
            if (connectionString != null)
            {
                ConnectionString = connectionString;
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
            if (bool.TryParse(isFirstRunStr, out isFirstRun)) {
                IsFirstRun = isFirstRun;
            }
        }

        public void Save() {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var app = config.AppSettings.Settings;

            // Add settings.
            app.Clear();
            app.Add("ConnectionString", ConnectionString);
            app.Add("ConsumerGroup", ConsumerGroup);
            app.Add("IsFirstRun", IsFirstRun.ToString());
            config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
