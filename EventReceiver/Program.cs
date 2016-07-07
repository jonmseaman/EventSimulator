using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            string eventHubConnectionString = "Endpoint=sb://servicebusintern2016.servicebus.windows.net/;SharedAccessKeyName=Managed;SharedAccessKey=oRt/agurnQYUDRsvm0tOKOEi2e5nXr0MnTrUxP8PdTw=";
            string eventHubName = "sellersite";
            string storageAccountName = "sbi2016ehreceiver";
            string storageAccountKey = "ijLlpwJryEx+o3dJIn2NDoUcvzZwVkhVSuPpQJezZjcnSGgVrwzOzpwaYF55b45AC8olef6MiltcB202mBMo4g==";
            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageAccountName, storageAccountKey);
            Console.WriteLine("Enter the consumer group name: ");
            string consumerGroupName = Console.ReadLine();

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, consumerGroupName, eventHubConnectionString, storageConnectionString);
            Console.WriteLine("Registering EventProcessor...");
            var options = new EventProcessorOptions();
            options.ExceptionReceived += (sender, e) => { Console.WriteLine(e.Exception); };
            
            eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>(options).Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
