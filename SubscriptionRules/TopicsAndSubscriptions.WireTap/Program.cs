using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;
using static System.Console;

var serviceBusConnectionString = Settings.GetConnectionString();
const string topicName = "Orders";

WriteLine("Wire Tap Console");
WriteLine("Press enter to activate wire tap");
ReadLine();

var subscriptionName = $"wiretap-{ Guid.NewGuid() }";

var managementClient = new ServiceBusAdministrationClient(serviceBusConnectionString);

var options = new CreateSubscriptionOptions(topicName, subscriptionName)
{
    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
};
var subscription = await managementClient.CreateSubscriptionAsync(options);

await using var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
await using var receiver = serviceBusClient.CreateReceiver(topicName, subscriptionName);

WriteLine($"Receiving on { subscriptionName }");
WriteLine("Press enter to quit...");

while(true)
{ 
    var message = await receiver.ReceiveMessageAsync();
    if(message != null)
    {
        InspectMessage(message);
        await receiver.CompleteMessageAsync(message);
    }
}

// await receiver.CloseAsync();

void InspectMessage(ServiceBusReceivedMessage message)
{
    WriteLine($"Received message...");

    WriteLine("Properties");
    WriteLine($"    ContentType             - { message.ContentType }");
    WriteLine($"    CorrelationId           - { message.CorrelationId }");
    WriteLine($"    Subject                 - { message.Subject }");
    WriteLine($"    MessageId               - { message.MessageId }");
    WriteLine($"    PartitionKey            - { message.PartitionKey }");
    WriteLine($"    ReplyTo                 - { message.ReplyTo }");
    WriteLine($"    ReplyToSessionId        - { message.ReplyToSessionId }");
    WriteLine($"    ScheduledEnqueueTime    - { message.ScheduledEnqueueTime }");
    WriteLine($"    SessionId               - { message.SessionId }");
    WriteLine($"    TimeToLive              - { message.TimeToLive }");
    WriteLine($"    To                      - { message.To }");

    WriteLine("ApplicationProperties");
    foreach (var property in message.ApplicationProperties)
    {
        WriteLine($"    { property.Key } - { property.Value }");
    }

    WriteLine("Body");
    WriteLine($"{ Encoding.UTF8.GetString(message.Body) }");
    WriteLine();
}