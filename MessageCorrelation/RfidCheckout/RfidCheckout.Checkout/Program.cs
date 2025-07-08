using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;
using Newtonsoft.Json;
using RfidCheckout.Messages;
using System.Reflection;
using System.Text;
using static System.Console;
using Spectre.Console;

var ReceivedCount = 0;
var BillTotal = 0.0;
var UseMessageSessions = false;

WriteLine("Checkout Console (receives messages)");

var connectionString = Settings.GetConnectionString(Assembly.GetExecutingAssembly());
const string queueName = "rfidcheckout";

var managementClient = new ServiceBusAdministrationClient(connectionString);

// Delete the queue if it exists.
if (await managementClient.QueueExistsAsync(queueName))
{
    WriteLine("Queue exists. Deleting...");
    await managementClient.DeleteQueueAsync(queueName);
}

var createQueueOptions = new CreateQueueOptions(queueName)
{
    // Comment in to require duplicate detection

    //RequiresDuplicateDetection = true,
    //DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10),

    // Comment in to require sessions

    //RequiresSession = true
};

WriteLine("Creating queue...");
await managementClient.CreateQueueAsync(createQueueOptions);

await using var queueClient = new ServiceBusClient(connectionString);

if (!UseMessageSessions)
{
    var processor = queueClient.CreateProcessor(queueName);

    processor.ProcessMessageAsync += ProcessOrderMessage;
    processor.ProcessErrorAsync += HandleMessageExceptions;

    await processor.StartProcessingAsync();

    WriteLine("Receiving tag read messages...");
    ReadLine();
    await processor.StopProcessingAsync();
    await processor.CloseAsync();

    AnsiConsole.MarkupInterpolated($"[green]Bill customer ${BillTotal} for {ReceivedCount} items.[/]");
    ReadLine();
}
else
{
    var sessionProcessorOptions = new ServiceBusSessionProcessorOptions
    {
        AutoCompleteMessages = false
    };
    await using var sessionProcessor = queueClient.CreateSessionProcessor(queueName, sessionProcessorOptions);

    sessionProcessor.ProcessMessageAsync += ProcessOrderSessionMessage;
    sessionProcessor.ProcessErrorAsync += HandleMessageExceptions;

    await sessionProcessor.StartProcessingAsync();
}

return;

Task ProcessOrderMessage(ProcessMessageEventArgs message)
{
    var rfidJson = Encoding.UTF8.GetString(message.Message.Body);
    var rfidTag = JsonConvert.DeserializeObject<RfidTag>(rfidJson);

    WriteLine($"{rfidTag}");

    ReceivedCount++;
    BillTotal += rfidTag.Price;
    
    return Task.CompletedTask;
}

async Task ProcessOrderSessionMessage(ProcessSessionMessageEventArgs  message)
{
    WriteLine("Accepting a message session...");
    WriteLine($"Accepted session: {message.Message.SessionId}");

    var receivedCount = 0;
    var billTotal = 0.0;

    // Process the order message
    var rfidJson = Encoding.UTF8.GetString(message.Message.Body);
    var rfidTag = JsonConvert.DeserializeObject<RfidTag>(rfidJson);
    WriteLine($"{rfidTag}");

    receivedCount++;
    billTotal += rfidTag.Price;

    await message.CompleteMessageAsync(message.Message);

    AnsiConsole.MarkupInterpolated($"[green]Bill customer ${billTotal} for {receivedCount} items.[/]");
}

Task HandleMessageExceptions(ProcessErrorEventArgs arg)
{
    throw new NotImplementedException();
}
