using System.Text;
using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;

string ServiceBusConnectionString = Settings.GetConnectionString();
string QueueName = "demoqueue";

var serviceBusClient = new ServiceBusClient(ServiceBusConnectionString);
var queueSender = serviceBusClient.CreateSender(QueueName);

for (int i = 0; i < 10; i++)
{
    var content = $"Message: { i }";
    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(content));
    queueSender.SendMessageAsync(message).Wait();
    Console.WriteLine("Sent: " + i);
}

queueSender.CloseAsync().Wait();
Console.WriteLine("Sent messages...");
Console.ReadLine();