using Azure;
using Azure.Messaging.ServiceBus.Administration;
using Spectre.Console;

namespace AzServiceBusAdministration;

internal sealed class ManagementHelper(string connectionString)
{
    private readonly ServiceBusAdministrationClient _managementClient = new(connectionString);

    public async Task CreateQueueAsync(string queuePath)
    {
        Console.Write("Creating queue {0}...", queuePath);
        var createQueueOptions = GetQueueOptions(queuePath);
        var queueProps = await _managementClient.CreateQueueAsync(createQueueOptions);
        Console.WriteLine("Done!");
        DumpQueueProperties(queueProps);
    }

    public async Task DeleteQueueAsync(string queuePath)
    {
        Console.Write("Deleting queue {0}...", queuePath);
        await _managementClient.DeleteQueueAsync(queuePath);
        AnsiConsole.MarkupLine(":cloud: Done!");
    }

    public async Task ListQueuesAsync()
    {
        var queueDescriptions = _managementClient.GetQueuesAsync();
        Console.WriteLine("Listing queues...");
        await foreach (QueueProperties properties in queueDescriptions)
        {
            Console.WriteLine("\t{0}", properties.Name);
        }
        Console.WriteLine("Done!");
    }

    public async Task GetQueueAsync(string queuePath)
    {
        var properties = await _managementClient.GetQueueAsync(queuePath);
        Console.WriteLine($"Queue description for { queuePath }");
        DumpQueueProperties(properties);
    }

    private void DumpQueueProperties(Response<QueueProperties> response)
    {
        var value = response.Value;
        
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        
        grid.AddRow("Name:", value.Name);
        grid.AddRow("MaxSizeInMegabytes:", value.MaxSizeInMegabytes.ToString());
        grid.AddRow("RequiresSession", value.RequiresSession.ToString());
        grid.AddRow("RequiresDuplicateDetection", value.RequiresDuplicateDetection.ToString());
        grid.AddRow("DuplicateDetectionHistoryTimeWindow", value.DuplicateDetectionHistoryTimeWindow.ToString());
        grid.AddRow("LockDuration", value.LockDuration.ToString());
        grid.AddRow("DefaultMessageTimeToLive", value.DefaultMessageTimeToLive.ToString());
        grid.AddRow("EnableDeadLetteringOnMessageExpiration", value.DeadLetteringOnMessageExpiration.ToString());
        grid.AddRow("EnableBatchedOperations", value.EnableBatchedOperations.ToString());
        grid.AddRow("MaxDeliveryCount", value.MaxDeliveryCount.ToString());
        grid.AddRow("Status", value.Status.ToString());
        
        AnsiConsole.Write(grid);
    }

    public async Task CreateTopicAsync(string topicPath)
    {
        Console.Write("Creating topic {0}...", topicPath);
        var response = await _managementClient.CreateTopicAsync(topicPath);
        AnsiConsole.MarkupLine($":cloud: Done! {response}");
    }
    
    public async Task CreateSubscriptionAsync(string topicPath, string subscriptionName)
    {
        Console.Write("Creating subscription {0}/subscriptions/{1}...", topicPath, subscriptionName);
        var description = await _managementClient.CreateSubscriptionAsync(topicPath, subscriptionName);
        Console.WriteLine("Done!");
    }
    
    public async Task ListTopicsAndSubscriptionsAsync()
    {
        var topics = _managementClient.GetTopicsAsync();
        Console.WriteLine("Listing topics and subscriptions...");
        await foreach (TopicProperties topic in topics)
        {
            Console.WriteLine("\t{0}", topic.Name);
            var subscriptions = _managementClient.GetSubscriptionsAsync(topic.Name);
            await foreach (SubscriptionProperties subscriptionDescription in subscriptions)
            {
                Console.WriteLine("\t\t{0}", subscriptionDescription.SubscriptionName);
            }
        }
        Console.WriteLine("Done!");
    }

    private CreateQueueOptions GetQueueOptions(string path)
    {
        return new CreateQueueOptions(path)
        {
            RequiresDuplicateDetection = true,
            //DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10),
            //RequiresSession = true,
            MaxDeliveryCount = 20,
            //DefaultMessageTimeToLive = TimeSpan.FromHours(1),
            //EnableDeadLetteringOnMessageExpiration = true
        };
    }
}