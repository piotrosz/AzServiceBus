using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Spectre.Console;

namespace TopicsAndSubscriptions;

class SubscriptionReceiver
{
    private readonly ServiceBusClient _client;
    private ServiceBusProcessor? _processor;

    public SubscriptionReceiver(string connectionString)
    {
        _client = new ServiceBusClient(connectionString);
    }

    public async Task RegisterMessageHandler(string topicName, string subscriptionName)
    {
        var options = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        };

        _processor = _client.CreateProcessor(topicName, subscriptionName, options);
        _processor.ProcessMessageAsync += ProcessOrderMessageMessageAsync;
        _processor.ProcessErrorAsync += ExceptionReceivedHandler;

        await _processor.StartProcessingAsync();
    }    private async Task ProcessOrderMessageMessageAsync(ProcessMessageEventArgs message)
    {
        // Process the order message
        var orderJson = Encoding.UTF8.GetString(message.Message.Body);
        var order = JsonConvert.DeserializeObject<Order>(orderJson);

        if (order != null)
        {
            AnsiConsole.MarkupLine($"[green]{order}[/]");
        }

        await message.CompleteMessageAsync(message.Message);
    }

    private Task ExceptionReceivedHandler(ProcessErrorEventArgs exceptionReceivedEventArgs)
    {
        AnsiConsole.MarkupLine($"[red]Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.[/]");
        return Task.CompletedTask;
    }    public async Task Close()
    {
        if (_processor != null)
        {
            await _processor.StopProcessingAsync();
            await _processor.CloseAsync();
        }
        await _client.DisposeAsync();
    }
}