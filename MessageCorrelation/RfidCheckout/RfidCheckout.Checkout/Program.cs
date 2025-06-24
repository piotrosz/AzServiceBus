using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;
using Newtonsoft.Json;
using RfidCheckout.Messages;
using System.Reflection;
using System.Text;
using static System.Console;

int ReceivedCount = 0;
double BillTotal = 0.0;
bool UseMessageSessions = false;

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

if (!UseMessageSessions)
{
    await using var queueClient = new ServiceBusClient(connectionString);
   
    var processor = queueClient.CreateProcessor(queueName);
    processor.ProcessMessageAsync += HandleMessage;
    processor.ProcessErrorAsync += HandleMessageExceptions;

    await processor.StartProcessingAsync();

    WriteLine("Receiving tag read messages...");
    ForegroundColor = ConsoleColor.Yellow;
    ReadLine();
    await processor.StopProcessingAsync();
    await processor.CloseAsync();

    // Bill the customer.
    ForegroundColor = ConsoleColor.Green;
    WriteLine("Bill customer ${0} for {1} items.", BillTotal, ReceivedCount);

    ReadLine();
}
else
{
    await using var queueClient = new ServiceBusClient(connectionString);

    var sessionProcessorOptions = new ServiceBusSessionProcessorOptions
    {
        AutoCompleteMessages = false
    };
    await using var sessionProcessor = queueClient.CreateSessionProcessor(queueName, sessionProcessorOptions);

    sessionProcessor.ProcessMessageAsync += ProcessSessionMessageHandler;
    sessionProcessor.ProcessErrorAsync += HandleMessageExceptions;

    await sessionProcessor.StartProcessingAsync();
}

return;

Task HandleMessage(ProcessMessageEventArgs message)
{
    // Process the order message
    var rfidJson = Encoding.UTF8.GetString(message.Message.Body);
    var rfidTag = JsonConvert.DeserializeObject<RfidTag>(rfidJson);

    WriteLine($"{rfidTag}");

    ReceivedCount++;
    BillTotal += rfidTag.Price;
    
    return Task.CompletedTask;
}

async Task ProcessSessionMessageHandler(ProcessSessionMessageEventArgs  message)
{
    WriteLine("Accepting a message session...");
    ForegroundColor = ConsoleColor.White;

    ForegroundColor = ConsoleColor.Cyan;
    WriteLine($"Accepted session: { message.Message.SessionId }");
    ForegroundColor = ConsoleColor.Yellow;

    int receivedCount = 0;
    double billTotal = 0.0;

    // Process the order message
    var rfidJson = Encoding.UTF8.GetString(message.Message.Body);
    var rfidTag = JsonConvert.DeserializeObject<RfidTag>(rfidJson);
    WriteLine($"{rfidTag}");

    receivedCount++;
    billTotal += rfidTag.Price;

    await message.CompleteMessageAsync(message.Message);

    // Bill the customer.
    ForegroundColor = ConsoleColor.Green;
    WriteLine("Bill customer ${0} for {1} items.", billTotal, receivedCount);                   

}

Task HandleMessageExceptions(ProcessErrorEventArgs arg)
{
    throw new NotImplementedException();
}
