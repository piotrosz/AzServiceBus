using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;
using Newtonsoft.Json;
using Spectre.Console;
using WorkingWithMessages.MessageEntities;

const string queueName = "workingwithmessages";

var connectionString = Settings.GetConnectionString(Assembly.GetExecutingAssembly());

await using var serviceBusClient = new ServiceBusClient(connectionString);

AnsiConsole.MarkupLine("[white]Sender Console - Hit enter[/]");
AnsiConsole.Prompt(new TextPrompt<string>("").AllowEmpty());

//TODO: Comment in the appropriate method

//await SendTextString("The quick brown fox jumps over the lazy dog", CancellationToken.None, serviceBusClient);

await SendPizzaOrderAsync(serviceBusClient, CancellationToken.None);
// await SendControlMessageAsync(serviceBusClient, CancellationToken.None);
// await SendPizzaOrderListAsMessagesAsync(serviceBusClient, CancellationToken.None);
// await SendPizzaOrderListAsBatchAsync(serviceBusClient, CancellationToken.None);
// await SendTextStringAsMessagesAsync(serviceBusClient, "The quick brown fox jumps over the lazy dog", CancellationToken.None);
// await SendTextStringAsBatchAsync(serviceBusClient, "The quick brown fox jumps over the lazy dog", CancellationToken.None);

AnsiConsole.MarkupLine("[white]Sender Console - Complete[/]");
AnsiConsole.Prompt(new TextPrompt<string>("").AllowEmpty());

return;

async Task SendTextString(string text, CancellationToken cancellationToken, ServiceBusClient client)
{
    AnsiConsole.MarkupLine("[cyan]SendTextStringAsMessagesAsync[/]");
    
    var sender = client.CreateSender(queueName);

    AnsiConsole.Markup("[lime]Sending...[/]");

    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(text));
    await sender.SendMessageAsync(message, cancellationToken);
    AnsiConsole.MarkupLine("[lime]Done![/]");

    AnsiConsole.WriteLine();
    await sender.CloseAsync();
}

async Task SendTextStringAsMessagesAsync(ServiceBusClient client, string text, CancellationToken cancellationToken)
{  
    AnsiConsole.MarkupLine("[cyan]SendTextStringAsMessagesAsync[/]");
    var sender = client.CreateSender(queueName);

    AnsiConsole.Markup("[lime]Sending:[/]");

    foreach (var letter in text.ToCharArray())
    {
        var message = new ServiceBusMessage
        {
            Subject = letter.ToString()
        };
        await sender.SendMessageAsync(message);
        AnsiConsole.Markup($"[lime]{message.MessageId.EscapeMarkup()}[/]");
    }

    AnsiConsole.WriteLine();
    AnsiConsole.WriteLine();

    await sender.CloseAsync();
}

async Task SendTextStringAsBatchAsync(ServiceBusClient client, string text, CancellationToken cancellationToken)
{
    AnsiConsole.MarkupLine("[cyan]SendTextStringAsBatchAsync[/]");

    var sender = client.CreateSender(queueName);

    AnsiConsole.Markup("[lime]Sending:[/]");
    var messageList = text
        .ToCharArray()
        .Select(letter => new ServiceBusMessage { Subject = letter.ToString() })
        .ToList();

    await sender.SendMessagesAsync(messageList);

    AnsiConsole.WriteLine();
    AnsiConsole.WriteLine();

    await sender.CloseAsync(cancellationToken);
}

async Task SendControlMessageAsync(ServiceBusClient client, CancellationToken cancellationToken)
{
    AnsiConsole.MarkupLine("[cyan]SendControlMessageAsync[/]");

    var message = new ServiceBusMessage
    {
        MessageId = "Control"
    };

    message.ApplicationProperties.Add("SystemId", 1462);
    message.ApplicationProperties.Add("Command", "Pending Restart");
    message.ApplicationProperties.Add("ActionTime", DateTime.UtcNow.AddHours(2));
    
    var sender = client.CreateSender(queueName);
    AnsiConsole.Markup("[lime]Sending control message...[/]");
    await sender.SendMessageAsync(message, cancellationToken);
    AnsiConsole.MarkupLine("[lime]Done![/]");
    AnsiConsole.WriteLine();
    await sender.CloseAsync(cancellationToken);
}

async Task SendPizzaOrderAsync(ServiceBusClient client, CancellationToken cancellationToken)
{
    AnsiConsole.MarkupLine("[cyan]SendPizzaOrderAsync[/]");

    var order = new PizzaOrder
    {
        CustomerName = "Alan Smith",
        Type = "Hawaiian",
        Size = "Large"
    };

    var jsonPizzaOrder = JsonConvert.SerializeObject(order);

    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonPizzaOrder))
    {
        MessageId = "PizzaOrder",
        ContentType = "application/json"
    };
    
    var sender = client.CreateSender(queueName);
    AnsiConsole.Markup("[lime]Sending order...[/]");
    await sender.SendMessageAsync(message, cancellationToken);
    AnsiConsole.MarkupLine("[lime]Done![/]");
    AnsiConsole.WriteLine();
    await sender.CloseAsync(cancellationToken);
}

async Task SendPizzaOrderListAsMessagesAsync(ServiceBusClient client, CancellationToken cancellationToken)
{
    AnsiConsole.MarkupLine("[cyan]SendPizzaOrderListAsMessagesAsync[/]");

    var pizzaOrderList = GetPizzaOrderList();
    var sender = client.CreateSender(queueName);

    AnsiConsole.MarkupLine("[yellow]Sending...[/]");
    var watch = Stopwatch.StartNew();

    foreach (var pizzaOrder in pizzaOrderList)
    {
        var jsonPizzaOrder = JsonConvert.SerializeObject(pizzaOrder);
        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonPizzaOrder))
        {
            
            Subject = "PizzaOrder",
            ContentType = "application/json"
        };
        await sender.SendMessageAsync(message, cancellationToken);
    }
    await sender.CloseAsync(cancellationToken);
    AnsiConsole.MarkupLineInterpolated($"[lime]Sent {pizzaOrderList.Count} orders! - Time: {watch.ElapsedMilliseconds} milliseconds, that's {pizzaOrderList.Count / watch.Elapsed.TotalSeconds} messages per second.[/]");

    AnsiConsole.WriteLine();
    AnsiConsole.WriteLine();
}

async Task SendPizzaOrderListAsBatchAsync(ServiceBusClient client,CancellationToken cancellationToken)
{
    AnsiConsole.MarkupLine("[cyan]SendPizzaOrderListAsBatchAsync[/]");

    var pizzaOrderList = GetPizzaOrderList();
    var sender = client.CreateSender(queueName);

    var watch = Stopwatch.StartNew();
    var messageList = new List<ServiceBusMessage>();

    foreach (var pizzaOrder in pizzaOrderList)
    {
        var jsonPizzaOrder = JsonConvert.SerializeObject(pizzaOrder);
        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonPizzaOrder))
        {
            Subject = "PizzaOrder",
            ContentType = "application/json"
        };
        messageList.Add(message);    }

    AnsiConsole.MarkupLine("[yellow]Sending...[/]");
    // This sends messages in a batch
    await sender.SendMessagesAsync(messageList, cancellationToken);
    await sender.CloseAsync();

    AnsiConsole.MarkupLineInterpolated($"[lime]Sent {pizzaOrderList.Count} orders! - Time: {watch.ElapsedMilliseconds} milliseconds, that's {pizzaOrderList.Count / watch.Elapsed.TotalSeconds} messages per second.[/]");
    AnsiConsole.WriteLine();
    AnsiConsole.WriteLine();
}

static List<PizzaOrder> GetPizzaOrderList()
{
    string[] names = [ "Alan", "Jennifer", "James" ];
    string[] pizzas = [ "Hawaiian", "Vegetarian", "Capricciosa", "Napolitana" ];

    var pizzaOrderList = new List<PizzaOrder>();
    foreach (var pizzaType in pizzas)
    {
        pizzaOrderList.AddRange(names.Select(name => new PizzaOrder
        {
            CustomerName = name, 
            Type = pizzaType, 
            Size = "Large"
        }));
    }
    return pizzaOrderList;
}
