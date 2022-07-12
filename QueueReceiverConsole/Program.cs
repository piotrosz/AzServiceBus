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
var receiver = serviceBusClient.CreateReceiver(QueueName, new ServiceBusReceiverOptions
{
    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
});

var messages = receiver.ReceiveMessagesAsync();



await foreach (var message in messages)
{
    var text = Encoding.UTF8.GetString(message.Body);
    
    Console.WriteLine($"Received: { text }");
}

Console.WriteLine("Press enter to exit.");
Console.ReadLine();

receiver.CloseAsync().Wait();