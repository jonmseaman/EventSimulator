using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace EventSimulator
{
    public class Sender
    {
        public Sender(string connectionString)
        {
            ConnectionString = connectionString;
            EventHubClient = EventHubClient.CreateFromConnectionString(connectionString);
        }

        #region Send

        public void Send(object toSend)
        {
            // Serialize
            var serializedString = JsonConvert.SerializeObject(toSend);
            // Make eventData
            var ed = new EventData(Encoding.UTF8.GetBytes(serializedString));
            // Send via client
            EventHubClient.Send(ed);
        }
        public async Task SendAsync(object toSend)
        {
            // Serialize
            var serializedString = JsonConvert.SerializeObject(toSend);
            // Make eventData
            var ed = new EventData(Encoding.UTF8.GetBytes(serializedString));
            // Send via client
            await EventHubClient.SendAsync(ed);
        }

        public void SendBatch(IEnumerable<object> toSend)
        {
            // Serialize and make event data
            var edList = ConvertToEventData(toSend);
            // Send via client
            EventHubClient.SendBatch(edList);

        }
        public async Task SendBatchAsync(IEnumerable<object> toSend)
        {
            // Serialize and make event data
            var edList = ConvertToEventData(toSend);
            // Send via client
            await EventHubClient.SendBatchAsync(edList);
        }
        private static List<EventData>  ConvertToEventData(IEnumerable<object> list)
        {
            // Initialize list to capacity = #objects in batch.
            var edList = new List<EventData>(list.Count());
            foreach (object obj in list)
            {
                edList.Add(new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj))));
            }
            return edList;
        }

        #endregion


        public EventHubClient EventHubClient;
        public string ConnectionString { get; private set; }

    }
}
