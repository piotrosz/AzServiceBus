using System.Text;
using WorkingWithMessages.MessageEntities;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;
using Newtonsoft.Json;
using Spectre.Console;

await using var queueClient = new ServiceBusClient(Settings.GetConnectionString());
const string queueName = "workingwithmessages";

AnsiConsole.MarkupLine("[bold white]Receiver Console[/]");

await RecreateQueueAsync();

//Comment in the appropriate method

await ReceiveAndProcessText(1);

//await ReceiveAndProcessPizzaOrders(1);
//await ReceiveAndProcessPizzaOrders(5);
//await ReceiveAndProcessPizzaOrders(100);

//await ReceiveAndProcessControlMessage(1);

//await ReceiveAndProcessCharacters(1);

//await ReceiveAndProcessCharacters(16);

return;

async Task ReceiveAndProcessText(int threads)
{
    AnsiConsole.MarkupLine($"[cyan]ReceiveAndProcessText({threads})[/]");

    var options = new ServiceBusProcessorOptions
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(30)
    };

    var processor = queueClient.CreateProcessor(queueName, options);

    processor.ProcessMessageAsync += ProcessTextMessageAsync;
    processor.ProcessErrorAsync += ProcessErrorHandler;

    AnsiConsole.WriteLine("Start processing");
    await processor.StartProcessingAsync();

    AnsiConsole.MarkupLine("[white]Receiving, hit enter to exit[/]");
    Console.ReadLine();
    await processor.StopProcessingAsync();
    await processor.CloseAsync();
}

async Task ReceiveAndProcessControlMessage(int threads)
{
    AnsiConsole.MarkupLine($"[cyan]ReceiveAndProcessPizzaOrders({ threads })[/]");
    
    var options = new ServiceBusProcessorOptions
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(30)
    };
    
    var processor = queueClient.CreateProcessor(queueName, options);

    processor.ProcessMessageAsync += ProcessControlMessageAsync;
    processor.ProcessErrorAsync += ProcessErrorHandler;
    
    AnsiConsole.MarkupLine("[white]Receiving, hit enter to exit[/]");
    Console.ReadLine();
    await processor.CloseAsync();
}

async Task ReceiveAndProcessPizzaOrders(int threads)
{
    AnsiConsole.MarkupLine($"[cyan]ReceiveAndProcessPizzaOrders({ threads })[/]");
    
    var options = new ServiceBusProcessorOptions()
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10)
    };

    var processor = queueClient.CreateProcessor(queueName, options);
    
    processor.ProcessMessageAsync += ProcessPizzaMessageAsync;
    processor.ProcessErrorAsync += ProcessErrorHandler;
    await processor.StartProcessingAsync();

    AnsiConsole.MarkupLine("[white]Receiving, hit enter to exit[/]");
    Console.ReadLine();
    await processor.StopProcessingAsync();
    await processor.CloseAsync();
}

async Task ProcessPizzaMessageAsync(ProcessMessageEventArgs message)
{
    var messageBodyText = Encoding.UTF8.GetString(message.Message.Body);

    var pizzaOrder = JsonConvert.DeserializeObject<PizzaOrder>(messageBodyText);

    if (pizzaOrder != null)
    {
        CookPizza(pizzaOrder);
    }
    else
    {
        AnsiConsole.MarkupLine("[red]Error: Could not deserialize pizza order[/]");
    }

    await message.CompleteMessageAsync(message.Message);
}

async Task ProcessTextMessageAsync(ProcessMessageEventArgs message)
{
    var messageBodyText = Encoding.UTF8.GetString(message.Message.Body);

    AnsiConsole.MarkupLine($"[green]Received: {messageBodyText.EscapeMarkup()}[/]");

    await message.CompleteMessageAsync(message.Message);
}

async Task ProcessControlMessageAsync(ProcessMessageEventArgs message)
{
    AnsiConsole.MarkupLine($"[green]Received: {message.Message.Subject.EscapeMarkup()}[/]");

    AnsiConsole.MarkupLine("[yellow]User properties...[/]");
    foreach (var property in message.Message.ApplicationProperties)
    {
        AnsiConsole.MarkupLine($"[cyan]    {property.Key.EscapeMarkup()} - {property.Value}[/]");
    }

    await message.CompleteMessageAsync(message.Message);
}

Task ProcessErrorHandler(ProcessErrorEventArgs exceptionReceivedEventArgs)
{
    AnsiConsole.MarkupLine($"[red]{exceptionReceivedEventArgs.Exception.Message.EscapeMarkup()}[/]");
    return Task.CompletedTask;
}

async Task ReceiveAndProcessCharacters(int threads)
{
    AnsiConsole.MarkupLine($"[cyan]ReceiveAndProcessCharacters({ threads })[/]");
    
    var options = new ServiceBusProcessorOptions
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(30)
    };

    var processor= queueClient.CreateProcessor(queueName, options);

    processor.ProcessMessageAsync += ProcessCharacterMessageAsync;
    processor.ProcessErrorAsync += ProcessErrorHandler;
    await processor.StartProcessingAsync();

    AnsiConsole.MarkupLine("[white]Receiving, hit enter to exit[/]");
    Console.ReadLine();
    await processor.StopProcessingAsync();
    await processor.CloseAsync();
}

async Task ProcessCharacterMessageAsync(ProcessMessageEventArgs message)
{
    AnsiConsole.Markup($"[green]{message.Message.Subject.EscapeMarkup()}[/]");
    await message.CompleteMessageAsync(message.Message);
}

static async Task RecreateQueueAsync()
{
    var manager = new ServiceBusAdministrationClient(Settings.GetConnectionString());
    if (await manager.QueueExistsAsync(queueName))
    {
        AnsiConsole.MarkupLine($"[magenta]Deleting queue: {queueName.EscapeMarkup()}...[/]");
        await manager.DeleteQueueAsync(queueName);
        AnsiConsole.MarkupLine("[magenta]Done![/]");
    }

    AnsiConsole.MarkupLine($"[magenta]Creating queue: {queueName.EscapeMarkup()}...[/]");
    await manager.CreateQueueAsync(queueName);
    AnsiConsole.MarkupLine("[magenta]Done![/]");
}

static void CookPizza(PizzaOrder order)
{
    AnsiConsole.MarkupLine($"[yellow]Cooking {order.Type.EscapeMarkup()} for {order.CustomerName.EscapeMarkup()}.[/]");
    Thread.Sleep(5000);
    AnsiConsole.MarkupLine($"[green]    {order.Type.EscapeMarkup()} pizza for {order.CustomerName.EscapeMarkup()} is ready![/]");
}

