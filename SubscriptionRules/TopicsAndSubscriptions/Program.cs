﻿using CommonServiceBusConnectionString;
using Spectre.Console;
using System.Reflection;
using TopicsAndSubscriptions;

var serviceBusConnectionString = Settings.GetConnectionString(Assembly.GetExecutingAssembly());
const string topicName = "Orders";

AnsiConsole.MarkupLine("[blue]Topics and Subscriptions Console[/]");

PromptAndWait("Press enter to create topic and subscriptions...");
await CreateTopicsAndSubscriptions();

PromptAndWait("Press enter to send order messages...");
await SendOrderMessages();

PromptAndWait("Press enter to receive order messages...");
await ReceiveOrdersFromAllSubscriptions();

PromptAndWait("Topics and Subscriptions Console Complete");

return;

async Task CreateTopicsAndSubscriptions()
{
    var manager = new SubscriptionsManager(serviceBusConnectionString);
   
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
    var orders = TestOrders.CreateTestOrders();

    var sender = new TopicSender(serviceBusConnectionString, topicName);

    foreach (var order in orders)
    {
        await sender.SendOrderMessage(order);
    }

    await sender.Close();
}

async Task ReceiveOrdersFromAllSubscriptions()
{
    var manager = new SubscriptionsManager(serviceBusConnectionString);

    // Loop through the subscriptions and process the order messages.
    await foreach (var subscriptionProperties in manager.GetSubscriptionsForTopic(topicName))
    {
        var receiver = new SubscriptionReceiver(serviceBusConnectionString);
        await receiver.RegisterMessageHandler(topicName, subscriptionProperties.SubscriptionName);
        PromptAndWait($"Receiving orders from { subscriptionProperties.SubscriptionName }, press enter when complete..");
        await receiver.Close();
    }
}

static void PromptAndWait(string text)
{
    AnsiConsole.MarkupLineInterpolated($"[cyan]{text}[/]");
    Console.ReadLine();
}