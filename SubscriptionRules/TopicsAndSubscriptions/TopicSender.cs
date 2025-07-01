using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Spectre.Console;

namespace TopicsAndSubscriptions;

sealed class TopicSender
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public TopicSender(string connectionString, string topicName)
    {
        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(topicName);
    }

    public async Task SendOrderMessage(Order order)
    {
        AnsiConsole.MarkupLine($"[yellow]{order}[/]");

        var orderJson = JsonConvert.SerializeObject(order);

        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(orderJson));

        message.ApplicationProperties.Add("region", order.Region);
        message.ApplicationProperties.Add("items", order.Items);
        message.ApplicationProperties.Add("value", order.Value);
        message.ApplicationProperties.Add("loyalty", order.HasLoyaltyCard);

        message.CorrelationId = order.Region;

        await _sender.SendMessageAsync(message);
    }

    public async Task Close()
    {
        await _sender.CloseAsync();
        await _client.DisposeAsync();
    }
}