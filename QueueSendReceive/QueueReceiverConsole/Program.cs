using System.Text;
using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;

var serviceBusConnectionString = Settings.GetConnectionString();
const string queueName = "demoqueue";

var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
var receiver = serviceBusClient.CreateReceiver(queueName, new ServiceBusReceiverOptions
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

await receiver.CloseAsync();