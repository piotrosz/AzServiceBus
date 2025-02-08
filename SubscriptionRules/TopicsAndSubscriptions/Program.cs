using CommonServiceBusConnectionString;
using TopicsAndSubscriptions;

var serviceBusConnectionString = Settings.GetConnectionString();
const string topicName = "Orders";

Console.WriteLine("Topics and Subscriptions Console");

PromptAndWait("Press enter to create topic and subscriptions...");
await CreateTopicsAndSubscriptions();

PromptAndWait("Press enter to send order messages...");
await SendOrderMessages();

PromptAndWait("Press enter to receive order messages...");
await ReceiveOrdersFromAllSubscriptions();

PromptAndWait("Topics and Subscriptions Console Complete");

async Task CreateTopicsAndSubscriptions()
{
    var manager = new Manager(serviceBusConnectionString);
   
    await manager.CreateTopic(topicName);
    await manager.CreateSubscription(topicName, "AllOrders");

    await manager.CreateSubscriptionWithSqlFilter(topicName, "UsaOrders", "region = 'USA'");
    await manager.CreateSubscriptionWithSqlFilter(topicName, "EuOrders", "region = 'EU'");

    await manager.CreateSubscriptionWithSqlFilter(topicName, "LargeOrders", "items > 30");
    await manager.CreateSubscriptionWithSqlFilter(topicName, "HighValueOrders", "value > 500");

    await manager.CreateSubscriptionWithSqlFilter(topicName, "LoyaltyCardOrders", "loyalty = true AND region = 'USA'");

    await manager.CreateSubscriptionWithCorrelationFilter(topicName, "UkOrders", "UK");
}

async Task SendOrderMessages()
{
    var orders = CreateTestOrders();

    var sender = new TopicSender(serviceBusConnectionString, topicName);

    foreach (var order in orders)
    {
        await sender.SendOrderMessage(order);
    }

    await sender.Close();
}

async Task ReceiveOrdersFromAllSubscriptions()
{
    var manager = new Manager(serviceBusConnectionString);

    // Loop through the subscriptions and process the order messages.
    await foreach (var subscriptionProperties in manager.GetSubscriptionsForTopic(topicName))
    {
        var receiver = new SubscriptionReceiver(serviceBusConnectionString);
        await receiver.RegisterMessageHandler(topicName, subscriptionProperties.SubscriptionName);
        PromptAndWait($"Receiving orders from { subscriptionProperties.SubscriptionName }, press enter when complete..");
        await receiver.Close();
    }
}

static List<Order> CreateTestOrders()
{
    return
    [
        new()
        {
            Name = "Loyal Customer",
            Value = 19.99,
            Region = "USA",
            Items = 1,
            HasLoyaltyCard = true
        },

        new()
        {
            Name = "Large Order",
            Value = 49.99,
            Region = "USA",
            Items = 50,
            HasLoyaltyCard = false
        },

        new()
        {
            Name = "High Value",
            Value = 749.45,
            Region = "USA",
            Items = 45,
            HasLoyaltyCard = false
        },

        new()
        {
            Name = "Loyal Europe",
            Value = 49.45,
            Region = "EU",
            Items = 3,
            HasLoyaltyCard = true
        },

        new()
        {
            Name = "UK Order",
            Value = 49.45,
            Region = "UK",
            Items = 3,
            HasLoyaltyCard = false
        }
    ];
}

static void PromptAndWait(string text)
{
    var temp = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(text);
    Console.ForegroundColor = temp;
    Console.ReadLine();
}
