using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;

string ServiceBusConnectionString = Settings.GetConnectionString();
string TopicName = "chattopic";

Console.WriteLine("Enter your name:");
var userName = Console.ReadLine();

var manager = new ServiceBusAdministrationClient(ServiceBusConnectionString);

if (!await manager.TopicExistsAsync(TopicName))
{
    await manager.CreateTopicAsync(TopicName);
}

// Create a subscription for the user
var createSubscriptionOptions = new CreateSubscriptionOptions(TopicName, userName)
{
    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
};

if (!await manager.SubscriptionExistsAsync(TopicName, userName))
{
    await manager.CreateSubscriptionAsync(createSubscriptionOptions);
}

var topicClient = new ServiceBusClient(ServiceBusConnectionString);
var sender = topicClient.CreateSender(TopicName);

await using var processor = topicClient.CreateProcessor(TopicName, userName);

processor.ProcessMessageAsync += MessageHandler;
processor.ProcessErrorAsync += ErrorHandler;

Console.WriteLine("Start processing...");
await processor.StartProcessingAsync();


try
{
    // Send a message to say you are here
    await sender.SendMessageAsync(CreateHelloMessage(userName));

    while (true)
    {
        string text = Console.ReadLine() ?? "";
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

static async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"{args.Message.ApplicationProperties["UserName"]}> {body}");
    await args.CompleteMessageAsync(args.Message);
}

static Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}

ServiceBusMessage CreateHelloMessage(string? userName)
{
    var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes("Has entered the room..."));
    //serviceBusMessage.Subject = userName;
    serviceBusMessage.ApplicationProperties.Add("UserName", userName);
    return serviceBusMessage;
}

ServiceBusMessage CreateGoodbyeMessage(string? userName)
{
    var goodbyeMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes("Has left the building..."));
    goodbyeMessage.ApplicationProperties.Add("UserName", userName);
    return goodbyeMessage;
}