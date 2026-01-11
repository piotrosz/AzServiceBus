using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using System.Text;

const string blobContainerUri = "https://sa0piotr0eventhubs.blob.core.windows.net/blobcontainer";
// Create a blob container client that the event processor will use
var storageClient = new BlobContainerClient(
    new Uri(blobContainerUri),
    new DefaultAzureCredential());

// Create an event processor client to process events in the event hub
const string qualifiedNamespace = "";
const string eventHubName = "";

var processor = new EventProcessorClient(
    storageClient,
    EventHubConsumerClient.DefaultConsumerGroupName,
    qualifiedNamespace, 
    eventHubName,
    new DefaultAzureCredential());

// Register handlers for processing events and handling errors
processor.ProcessEventAsync += ProcessEventHandler;
processor.ProcessErrorAsync += ProcessErrorHandler;

await processor.StartProcessingAsync();

// Wait for 'x' key to be pressed to stop processing
Console.WriteLine("Press 'x' to stop processing events...");
while (true)
{
    if (Console.KeyAvailable)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.KeyChar is 'x' or 'X')
        {
            break;
        }
    }
    await Task.Delay(100); // Small delay to prevent CPU spinning
}

await processor.StopProcessingAsync();

return;

Task ProcessEventHandler(ProcessEventArgs eventArgs)
{
    var eventBody = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());
    // Write the body of the event to the console window
    Console.WriteLine("Received event: {0}", eventBody);
    return Task.CompletedTask;
}

Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
{
    // Write details about the error to the console window
    Console.WriteLine($"\tPartition '{eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
    Console.WriteLine(eventArgs.Exception.Message);
    return Task.CompletedTask;
}