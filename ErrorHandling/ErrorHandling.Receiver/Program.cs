using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;
using ErrorHandling.Receiver;
using Newtonsoft.Json;

Utils.WriteLine("ReceiverConsole", ConsoleColor.White);
Console.WriteLine();

await using var client = new ServiceBusClient(Settings.GetConnectionString());
var queueName = "errorhandling";
var forwardingQueue = "forwardingqueue";

await EnsureQueues();
await ReceiveMessages();


async Task ReceiveMessages()
{
    var options = new ServiceBusProcessorOptions
    {
        MaxConcurrentCalls = 1,
        AutoCompleteMessages = false
    };

    var processor = client.CreateProcessor(queueName, options);

    processor.ProcessMessageAsync += ProcessMessage;
    processor.ProcessErrorAsync += ProcessError;

    await processor.StartProcessingAsync();
    Utils.WriteLine("Receiving messages", ConsoleColor.Cyan);

    Console.ReadLine();
    await processor.StopProcessingAsync();
    await processor.CloseAsync();

}

async Task ProcessMessage(ProcessMessageEventArgs args)
{
    var message = args.Message;
    Utils.WriteLine("Received: " + message.ContentType, ConsoleColor.Cyan);

    switch (message.ContentType)
    {
        case "text/plain":
            await ProcessTextMessage(args);
            break;
        case "application/json":
            await ProcessJsonMessage(args);
            break;
        default:
            Console.WriteLine("Received unknown message: " + message.ContentType);

            // Comment in to abandon message
            //await args.AbandonMessageAsync(message);

            // Comment in to dead letter message
            await args.DeadLetterMessageAsync(
                message,
                "Unknown message type",
                "The message type: " + message.ContentType + " is not known.");

            break;
    }
}


async Task ProcessTextMessage(ProcessMessageEventArgs args)
{
    var body = Encoding.UTF8.GetString(args.Message.Body);

    Utils.WriteLine($"Text message: { body } - DeliveryCount: {args.Message.DeliveryCount}", ConsoleColor.Green);

    try
    {
        // Send a message to a forwarding queue
        // Disable forwarding queue to test for exception being thrown
        var forwardingMessage = new ServiceBusMessage();
        var forwardingSender = client.CreateSender(forwardingQueue);
        await forwardingSender.SendMessageAsync(forwardingMessage);
        await forwardingSender.CloseAsync();

        // Complete the message if successfully processed
        await args.CompleteMessageAsync(args.Message);

        Utils.WriteLine("Processed message", ConsoleColor.Cyan);
    }
    catch (Exception ex)
    {
        Utils.WriteLine($"Exception: {  ex.Message }", ConsoleColor.Yellow);

        // Comment in to abandon message
        //await args.AbandonMessageAsync(args.Message);

        // Comment in to dead-letter the message after 2 processing attempts
        if (args.Message.DeliveryCount > 2)
        {
            await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.ToString());
        }
        else
        {
            // Abandon the message
            await args.AbandonMessageAsync(args.Message);
        }
    }
}

async Task ProcessJsonMessage(ProcessMessageEventArgs args)
{
    Utils.WriteLine($"Message delivery count is {args.Message.DeliveryCount}", ConsoleColor.Green);
    
    var body = Encoding.UTF8.GetString(args.Message.Body);
    Utils.WriteLine($"JSON message body { body }" + body, ConsoleColor.Green);

    try
    {                
        dynamic data = JsonConvert.DeserializeObject(body);
        Utils.WriteLine($"      Name: { data.contact.name }", ConsoleColor.Green);
        Utils.WriteLine($"      Twitter: { data.contact.twitter }", ConsoleColor.Green);

        // Complete the message if successfully processed
        await args.CompleteMessageAsync(args.Message);
        Utils.WriteLine("Processed message", ConsoleColor.Cyan);
    }
    catch (Exception ex)
    {
        Utils.WriteLine($"Exception: {ex.Message}", ConsoleColor.Yellow);

        await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.ToString());

    }
}


async Task EnsureQueues()
{
    var managementClient = new ServiceBusAdministrationClient(Settings.GetConnectionString());
           
    if (!await managementClient.QueueExistsAsync(queueName))
    {
        await managementClient.CreateQueueAsync(new CreateQueueOptions(queueName)
        {
            LockDuration = TimeSpan.FromSeconds(5),
            MaxDeliveryCount = 5, // 10 is the default
            AutoDeleteOnIdle = TimeSpan.FromDays(5),
        });
    }
    if (!await managementClient.QueueExistsAsync(forwardingQueue))
    {
        await managementClient.CreateQueueAsync(forwardingQueue);
    }
}

async Task ProcessError(ProcessErrorEventArgs arg)
{
    Utils.WriteLine($"Exception: { arg.Exception.Message }", ConsoleColor.Yellow);
    
}