using CommonServiceBusConnectionString;
using TopicsAndSubscriptions;

string ServiceBusConnectionString = Settings.GetConnectionString();
string TopicName = "Orders";

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
    var manager = new Manager(ServiceBusConnectionString);
   
    await manager.CreateTopic(TopicName);
    await manager.CreateSubscription(TopicName, "AllOrders");

    await manager.CreateSubscriptionWithSqlFilter(TopicName, "UsaOrders", "region = 'USA'");
    await manager.CreateSubscriptionWithSqlFilter(TopicName, "EuOrders", "region = 'EU'");

    await manager.CreateSubscriptionWithSqlFilter(TopicName, "LargeOrders", "items > 30");
    await manager.CreateSubscriptionWithSqlFilter(TopicName, "HighValueOrders", "value > 500");

    await manager.CreateSubscriptionWithSqlFilter(TopicName, "LoyaltyCardOrders", "loyalty = true AND region = 'USA'");

    await manager.CreateSubscriptionWithCorrelationFilter(TopicName, "UkOrders", "UK");
}

async Task SendOrderMessages()
{
    var orders = CreateTestOrders();

    var sender = new TopicSender(ServiceBusConnectionString, TopicName);

    foreach (var order in orders)
    {
        await sender.SendOrderMessage(order);
    }

    await sender.Close();
}

async Task ReceiveOrdersFromAllSubscriptions()
{
    var manager = new Manager(ServiceBusConnectionString);

    // Loop through the subscriptions and process the order messages.
    await foreach (var subscriptionProperties in manager.GetSubscriptionsForTopic(TopicName))
    {
        var receiver = new SubscriptionReceiver(ServiceBusConnectionString);
        await receiver.RegisterMessageHandler(TopicName, subscriptionProperties.SubscriptionName);
        PromptAndWait($"Receiving orders from { subscriptionProperties.SubscriptionName }, press enter when complete..");
        await receiver.Close();
    }
}

static List<Order> CreateTestOrders()
{
    return new List<Order>
    {
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
    };
}

static void PromptAndWait(string text)
{
    var temp = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(text);
    Console.ForegroundColor = temp;
    Console.ReadLine();
}
