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

await CreateQueue();

ReceiveMessages();

Console.ReadLine();


void ReceiveMessages()
{
    var options = new ServiceBusProcessorOptions
    {
        MaxConcurrentCalls = 1,
        AutoCompleteMessages = false
    };

    var processor = client.CreateProcessor(queueName, options);

    processor.ProcessMessageAsync += ProcessMessage;
    processor.ProcessErrorAsync += ProcessError;

    Utils.WriteLine("Receiving messages", ConsoleColor.Cyan);

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
            //await args.DeadLetterMessageAsync(
            //    message, 
            //    "Unknown message type",
            //    "The message type: " + message.ContentType + " is not known.");

            break;
    }
}


async Task ProcessTextMessage(ProcessMessageEventArgs args)
{
    var body = Encoding.UTF8.GetString(args.Message.Body);

    Utils.WriteLine($"Text message: { body } - DeliveryCount: { args.Message.DeliveryCount }", ConsoleColor.Green);

    try
    {
        // Send a message to a queue
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

        // Comment in to dead-letter the message after 5 processing attempts
        //if (args.Message.DeliveryCount > 5)
        //{
        //    await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.ToString());
        //}
        //else
        //{
        //    // Abandon the message
        //    await args.AbandonMessageAsync(args.Message);
        //}
    }
}

async Task ProcessJsonMessage(ProcessMessageEventArgs args)
{
    var body = Encoding.UTF8.GetString(args.Message.Body);
    Utils.WriteLine($"JSON message { body }" + body, ConsoleColor.Green);

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
        Utils.WriteLine($"Exception: {  ex.Message }", ConsoleColor.Yellow);
        //await message.DeadLetterMessageAsync(message.Message, ex.Message, ex.ToString());

    }
}


async Task CreateQueue()
{
    var managementClient = new ServiceBusAdministrationClient(Settings.GetConnectionString());
           
    if (!await managementClient.QueueExistsAsync(queueName))
    {
        await managementClient.CreateQueueAsync(new CreateQueueOptions(queueName)
        {
            LockDuration = TimeSpan.FromSeconds(5)
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