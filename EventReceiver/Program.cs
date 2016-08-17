using System;
using System.Deployment.Application;

using Microsoft.ServiceBus.Messaging;

namespace Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Console.WriteLine($"Version: {ApplicationDeployment.CurrentDeployment.CurrentVersion}");
            }

            // Get settings.
            var settings = new Settings();
            settings.Load();

            // Default to running setup for the first run of the program.
            Console.Write($"Run setup? <{true}/{false}> ({settings.IsFirstRun}): ");
            var runSetupStr = Console.ReadLine();
            bool runSetup;
            if (!bool.TryParse(runSetupStr, out runSetup))
            {
                runSetup = settings.IsFirstRun;
            }

            if (runSetup)
            {
                GetSettingsFromUser(settings);
            }

            var eventProcessorHostName = Guid.NewGuid().ToString();
            var eventProcessorHost = new EventProcessorHost(eventProcessorHostName, settings.EventHubName, settings.ConsumerGroup, settings.ConnectionString, settings.StorageConnectionString);
            Console.WriteLine("Registering EventProcessor...");
            var options = new EventProcessorOptions();
            options.ExceptionReceived += (sender, e) => { Console.WriteLine(e.Exception); };
            
            eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>(options).Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }

        static void GetSettingsFromUser(Settings settings)
        {
            Console.WriteLine("Enter settings: ");

            // Get the connection string
            Console.Write($"ConnectionString ({settings.ConnectionString}): ");
            var connectionString = Console.ReadLine();
            if (!connectionString.Equals(string.Empty)) {
                settings.ConnectionString = connectionString;
            }

            // EventHubName
            Console.Write($"EventHubName ({settings.EventHubName}): ");
            var eventHubName = Console.ReadLine();
            if (!eventHubName.Equals(string.Empty)) {
                settings.EventHubName = eventHubName;
            }

            // Get the consumer group.
            Console.Write($"ConsumerGroup ({settings.ConsumerGroup}): ");
            var consumerGroup = Console.ReadLine();
            if (!consumerGroup.Equals(string.Empty)) {
                settings.ConsumerGroup = consumerGroup;
            }

            // StorageAccountName
            Console.Write($"StorageAccountName ({settings.StorageAccountName}): ");
            var storageAccountName = Console.ReadLine();
            if (!storageAccountName.Equals(string.Empty)) {
                settings.StorageAccountName = storageAccountName;
            }

            // StorageAccountKey
            Console.Write($"StorageAccountKey ({settings.StorageAccountKey}): ");
            var storageAccountKey = Console.ReadLine();
            if (!storageAccountKey.Equals(string.Empty)) {
                settings.StorageAccountKey = storageAccountKey;
            }

            // Would you like to save these settings?
            Console.Write($"Save settings for next run <{true}/{false}>: ");
            var saveSettingsStr = Console.ReadLine();
            bool shouldSave;
            bool.TryParse(saveSettingsStr, out shouldSave);

            if (shouldSave)
            {
                Console.Write("Saving...");
                settings.IsFirstRun = false;
                settings.Save();
                Console.WriteLine("Done.");
            }
            else
            {
                Console.WriteLine("Not saving.");
            }
        }

    }
}
