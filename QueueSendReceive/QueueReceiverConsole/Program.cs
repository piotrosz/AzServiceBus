using System.Text;
using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;

string ServiceBusConnectionString = Settings.GetConnectionString();
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