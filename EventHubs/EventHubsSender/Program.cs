using System;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;

// number of events to be sent to the event hub
var numOfEvents = 4;

// The Event Hubs client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when events are being published or read regularly.

const string namespaceName = "";
const string eventHubName = "";

await using var producerClient = new EventHubProducerClient(
    namespaceName,
    eventHubName,
    new DefaultAzureCredential());

// Create a batch of events 
using var eventBatch = await producerClient.CreateBatchAsync();

for (var i = 1; i <= numOfEvents; i++)
{
    if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes($"Test Event {i}"))))
    {
        // if it is too large for the batch
        throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
    }
}

// Use the producer client to send the batch of events to the event hub
await producerClient.SendAsync(eventBatch);
Console.WriteLine($"A batch of {numOfEvents} events has been published.");
Console.ReadLine();
