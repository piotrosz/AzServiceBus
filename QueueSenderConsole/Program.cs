using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);

IConfiguration config = builder.Build();
string ServiceBusConnectionString = config.GetConnectionString("ServiceBus");
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