using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;
using System.Reflection;
using System.Text;

var serviceBusConnectionString = Settings.GetConnectionString(Assembly.GetExecutingAssembly());
const string topicName = "chattopic";

Console.WriteLine("Enter your name:");
var userName = Console.ReadLine();

var manager = new ServiceBusAdministrationClient(serviceBusConnectionString);

if (!await manager.TopicExistsAsync(topicName))
{
    await manager.CreateTopicAsync(topicName);
}

// Create a subscription for the user
var createSubscriptionOptions = new CreateSubscriptionOptions(topicName, userName)
{
    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
};

if (!await manager.SubscriptionExistsAsync(topicName, userName))
{
    await manager.CreateSubscriptionAsync(createSubscriptionOptions);
}

var topicClient = new ServiceBusClient(serviceBusConnectionString);
var sender = topicClient.CreateSender(topicName);

await using var processor = topicClient.CreateProcessor(topicName, userName);

processor.ProcessMessageAsync += HandleMessage;
processor.ProcessErrorAsync += HandleError;

Console.WriteLine("Start processing...");
await processor.StartProcessingAsync();

try
{
    // Send a message to say you are here
    await sender.SendMessageAsync(CreateHelloMessage(userName));

    while (true)
    {
        var text = Console.ReadLine() ?? "";
        if (text.Equals("exit"))
        {
            break;
        }

        // Send a chat message
        var chatMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(text));
        chatMessage.ApplicationProperties.Add("UserName", userName);
        await sender.SendMessageAsync(chatMessage);
    }

    // Send a message to say you are leaving
    await sender.SendMessageAsync(CreateGoodbyeMessage(userName));
}
finally
{
    await processor.CloseAsync();
    await sender.CloseAsync();
}

return;

static async Task HandleMessage(ProcessMessageEventArgs args)
{
    var body = args.Message.Body.ToString();
    Console.WriteLine($"{args.Message.ApplicationProperties["UserName"]}> {body}");
    await args.CompleteMessageAsync(args.Message);
}

static Task HandleError(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}

ServiceBusMessage CreateHelloMessage(string? userName)
{
    var serviceBusMessage = new ServiceBusMessage("Has entered the room..."u8.ToArray());
    //serviceBusMessage.Subject = userName;
    serviceBusMessage.ApplicationProperties.Add("UserName", userName);
    return serviceBusMessage;
}

ServiceBusMessage CreateGoodbyeMessage(string? userName)
{
    var goodbyeMessage = new ServiceBusMessage("Has left the building..."u8.ToArray());
    goodbyeMessage.ApplicationProperties.Add("UserName", userName);
    return goodbyeMessage;
}