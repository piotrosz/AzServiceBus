using System.Text;
using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;

var serviceBusConnectionString = Settings.GetConnectionString();
const string queueName = "demoqueue";

var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
var queueSender = serviceBusClient.CreateSender(queueName);

for (var i = 0; i < 10; i++)
{
    var content = $"Message: { i }";
    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(content));
    queueSender.SendMessageAsync(message).Wait();
    Console.WriteLine("Sent: " + i);
}

await queueSender.CloseAsync();
Console.WriteLine("Sent messages...");
Console.ReadLine();