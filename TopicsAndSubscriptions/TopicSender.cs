using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace TopicsAndSubscriptions
{
    sealed class TopicSender
    {
        private readonly ServiceBusClient _client;
        private ServiceBusSender _sender;

        public TopicSender(string connectionString, string topicName)
        {
            _client = new ServiceBusClient(connectionString);
            _client.CreateSender(topicName);
        }

        public async Task SendOrderMessage(Order order)
        {
            Console.WriteLine($"{ order }");

            // Serialize the order to JSON
            var orderJson = JsonConvert.SerializeObject(order);

            // Create a message containing the serialized order Json
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(orderJson));

            // Promote properties...       
            message.ApplicationProperties.Add("region", order.Region);           
            message.ApplicationProperties.Add("items", order.Items);
            message.ApplicationProperties.Add("value", order.Value);            
            message.ApplicationProperties.Add("loyalty", order.HasLoyaltyCard);

            // Set the correlation Id
            message.CorrelationId = order.Region;

            // Send the message
            await _sender.SendMessageAsync(message);
        }

        public async Task Close()
        {
            await _sender.CloseAsync();
            await _client.DisposeAsync();
        }




    }
}
