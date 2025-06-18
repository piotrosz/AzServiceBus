using Azure;
using Azure.Messaging.ServiceBus.Administration;
using Spectre.Console;

internal sealed class SubscriptionsManager(string connectionString)
{
    private readonly ServiceBusAdministrationClient _administrationClient = new(connectionString);    public async Task<TopicProperties> CreateTopic(string topicName)
    {
        AnsiConsole.MarkupLine($"[blue]Creating Topic[/] [green]{ topicName }[/]");

        if (await _administrationClient.TopicExistsAsync(topicName))
        {
            await _administrationClient.DeleteTopicAsync(topicName);
        }

        return await _administrationClient.CreateTopicAsync(topicName);
    }    public async Task<SubscriptionProperties> CreateSubscription(string topicName, string subscriptionName)
    {
        AnsiConsole.MarkupLine($"[blue]Creating Subscription[/] [green]{ topicName }[/]/[yellow]{ subscriptionName }[/]");
        return await _administrationClient.CreateSubscriptionAsync(topicName, subscriptionName);
    }    public async Task<SubscriptionProperties> CreateSubscriptionWithSqlFilter(string topicName, string subscriptionName, string sqlExpression)
    {
        AnsiConsole.MarkupLine($"[blue]Creating Subscription with SQL Filter[/] [green]{ topicName }[/]/[yellow]{ subscriptionName }[/] ([magenta]{ sqlExpression }[/])");
        var createSubscriptionOptions = new CreateSubscriptionOptions(topicName, subscriptionName);
        var ruleDescription = new CreateRuleOptions("Default", new SqlRuleFilter(sqlExpression));
        return await _administrationClient.CreateSubscriptionAsync(createSubscriptionOptions, ruleDescription);
    }    public async Task<SubscriptionProperties> CreateSubscriptionWithCorrelationFilter(string topicName, string subscriptionName, string correlationId)
    {
        AnsiConsole.MarkupLine($"[blue]Creating Subscription with Correlation Filter[/] [green]{ topicName }[/]/[yellow]{ subscriptionName }[/] ([magenta]{ correlationId }[/])");
        var createSubscriptionOptions = new CreateSubscriptionOptions(topicName, subscriptionName);
        var ruleDescription = new CreateRuleOptions("Default", new CorrelationRuleFilter(correlationId));

        return await _administrationClient.CreateSubscriptionAsync(createSubscriptionOptions, ruleDescription);
    }
    public AsyncPageable<SubscriptionProperties> GetSubscriptionsForTopic(string topicName)
    {
        return _administrationClient.GetSubscriptionsAsync(topicName);
    }
}

