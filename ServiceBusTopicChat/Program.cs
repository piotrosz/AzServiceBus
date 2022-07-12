using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);

IConfiguration config = builder.Build();
string ServiceBusConnectionString = config.GetConnectionString("ServiceBus");
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

var processor = topicClient.CreateProcessor(TopicName, userName);
processor.ProcessMessageAsync += MessageHandler;
processor.ProcessErrorAsync += ErrorHandler;

try
{
    await processor.StartProcessingAsync();

    // Send a message to say you are here
    var helloMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes("Has entered the room..."));
    helloMessage.ApplicationProperties.Add("UserName", userName);
    await sender.SendMessageAsync(helloMessage);

    while (true)
    {
        string text = Console.ReadLine();
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
    var goodbyeMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes("Has left the building..."));
    goodbyeMessage.ApplicationProperties.Add("UserName", userName);
    await sender.SendMessageAsync(goodbyeMessage);
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