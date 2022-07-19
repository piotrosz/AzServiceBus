using Azure;
using Azure.Messaging.ServiceBus.Administration;

sealed class Manager
{
    private readonly ServiceBusAdministrationClient _administrationClient;
    public Manager(string connectionString)
    {
        _administrationClient = new ServiceBusAdministrationClient(connectionString);
    }

    public async Task<TopicProperties> CreateTopic(string topicName)
    {
        Console.WriteLine($"Creating Topic { topicName }");

        if (await _administrationClient.TopicExistsAsync(topicName))
        {
            await _administrationClient.DeleteTopicAsync(topicName);
        }

        return await _administrationClient.CreateTopicAsync(topicName);
    }

    public async Task<SubscriptionProperties> CreateSubscription(string topicName, string subscriptionName)
    {
        Console.WriteLine($"Creating Subscription { topicName }/{ subscriptionName }");
        return await _administrationClient.CreateSubscriptionAsync(topicName, subscriptionName);
    }

    public async Task<SubscriptionProperties> CreateSubscriptionWithSqlFilter(string topicName, string subscriptionName, string sqlExpression)
    {
        Console.WriteLine($"Creating Subscription with SQL Filter{ topicName }/{ subscriptionName } ({ sqlExpression })");
        var createSubscriptionOptions = new CreateSubscriptionOptions(topicName, subscriptionName);
        var ruleDescription = new CreateRuleOptions("Default", new SqlRuleFilter(sqlExpression));
        return await _administrationClient.CreateSubscriptionAsync(createSubscriptionOptions, ruleDescription);
    }

    public async Task<SubscriptionProperties> CreateSubscriptionWithCorrelationFilter(string topicName, string subscriptionName, string correlationId)
    {
        Console.WriteLine($"Creating Subscription with Correlation Filter{ topicName }/{ subscriptionName } ({ correlationId })");
        var createSubscriptionOptions = new CreateSubscriptionOptions(topicName, subscriptionName);
        var ruleDescription = new CreateRuleOptions("Default", new CorrelationRuleFilter(correlationId));

        return await _administrationClient.CreateSubscriptionAsync(createSubscriptionOptions, ruleDescription);
    }
    public AsyncPageable<SubscriptionProperties> GetSubscriptionsForTopic(string topicName)
    {
        return _administrationClient.GetSubscriptionsAsync(topicName);
    }
}

