using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;

using Newtonsoft.Json;
using Spectre.Console;

AnsiConsole.MarkupLine("[white]ReceiverConsole[/]");
Console.WriteLine();

await using var client = new ServiceBusClient(Settings.GetConnectionString());
const string queueName = "errorhandling";
const string forwardingQueue = "forwardingqueue";

await EnsureQueues();
await ReceiveMessages();

return;

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
    AnsiConsole.MarkupLine("[cyan]Receiving messages[/]");
    
    Console.ReadLine();
    await processor.StopProcessingAsync();
    await processor.CloseAsync();

}

async Task ProcessMessage(ProcessMessageEventArgs args)
{
    var message = args.Message;
    AnsiConsole.MarkupLineInterpolated($"[cyan]Received: {message.ContentType}[/]");
    
    switch (message.ContentType)
    {
        case "text/plain":
            await ProcessTextMessage(args);
            break;
        case "application/json":
            await ProcessJsonMessage(args);
            break;
        default:
            AnsiConsole.MarkupLineInterpolated($"[red]Received unknown message: {message.ContentType}[/]");

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

    AnsiConsole.MarkupLineInterpolated($"[green]Text message: {body} - DeliveryCount: {args.Message.DeliveryCount}[/]");
    
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

        AnsiConsole.MarkupLine("[cyan]Processed message[/]");
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
        
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
    AnsiConsole.MarkupLineInterpolated($"[green]Message delivery count is {args.Message.DeliveryCount}[/]");
    
    var body = Encoding.UTF8.GetString(args.Message.Body);
    AnsiConsole.MarkupLineInterpolated($"[green]JSON message body: {body}[/]");

    try
    {                
        dynamic data = JsonConvert.DeserializeObject(body);
        AnsiConsole.MarkupLineInterpolated($"[green]Name: {data.contact.name}[/]");
        AnsiConsole.MarkupLineInterpolated($"[green]Twitter: {data.contact.twitter}[/]");

        // Complete the message if successfully processed
        await args.CompleteMessageAsync(args.Message);
        AnsiConsole.MarkupLine("[cyan]Processed message[/]");
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
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

Task ProcessError(ProcessErrorEventArgs arg)
{
    AnsiConsole.WriteException(arg.Exception);
    return Task.CompletedTask;
}