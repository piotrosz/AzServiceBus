using System.Diagnostics;
using System.Reflection;
using System.Text;
using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;
using Newtonsoft.Json;
using Spectre.Console;
using WorkingWithMessages.MessageEntities;

const string queueName = "workingwithmessages";

AnsiConsole.MarkupLine("[white]Sender Console - Hit enter[/]");
AnsiConsole.Prompt(new TextPrompt<string>("").AllowEmpty());

//ToDo: Comment in the appropriate method

//await SendTextString("The quick brown fox jumps over the lazy dog");

//await SendPizzaOrderAsync();

//await SendControlMessageAsync();

//await SendPizzaOrderListAsMessagesAsync();
        
await SendPizzaOrderListAsBatchAsync();

//await SendTextStringAsMessagesAsync("The quick brown fox jumps over the lazy dog");

//await SendTextStringAsBatchAsync("The quick brown fox jumps over the lazy dog");


AnsiConsole.MarkupLine("[white]Sender Console - Complete[/]");
AnsiConsole.Prompt(new TextPrompt<string>("").AllowEmpty());

return;

static async Task SendTextString(string text)
{
    AnsiConsole.MarkupLine("[cyan]SendTextStringAsMessagesAsync[/]");

    await using var client = new ServiceBusClient(Settings.GetConnectionString(Assembly.GetExecutingAssembly()));
    var sender = client.CreateSender(queueName);

    AnsiConsole.Markup("[lime]Sending...[/]");

    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(text));
    await sender.SendMessageAsync(message);
    AnsiConsole.MarkupLine("[lime]Done![/]");

    AnsiConsole.WriteLine();

    await sender.CloseAsync();
}


static async Task SendTextStringAsMessagesAsync(string text)
{
    AnsiConsole.MarkupLine("[cyan]SendTextStringAsMessagesAsync[/]");

    // Create a client
    await using var client = new ServiceBusClient(Settings.GetConnectionString(Assembly.GetExecutingAssembly()));
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

static async Task SendTextStringAsBatchAsync(string text)
{
    AnsiConsole.MarkupLine("[cyan]SendTextStringAsBatchAsync[/]");

    await using var client = new ServiceBusClient(Settings.GetConnectionString(Assembly.GetExecutingAssembly()));
    var sender = client.CreateSender(queueName);

    AnsiConsole.Markup("[lime]Sending:[/]");
    
    var messageList = new List<ServiceBusMessage>();

    foreach (var letter in text.ToCharArray())
    {
        var message = new ServiceBusMessage
        {
            Subject = letter.ToString()
        };

        messageList.Add(message);

    }    await sender.SendMessagesAsync(messageList);

    AnsiConsole.WriteLine();
    AnsiConsole.WriteLine();

    await sender.CloseAsync();
}

static async Task SendControlMessageAsync()
{
    AnsiConsole.MarkupLine("[cyan]SendControlMessageAsync[/]");

    var message = new ServiceBusMessage()
    {
        MessageId = "Control"
    };

    message.ApplicationProperties.Add("SystemId", 1462);
    message.ApplicationProperties.Add("Command", "Pending Restart");
    message.ApplicationProperties.Add("ActionTime", DateTime.UtcNow.AddHours(2));

    await using var client = new ServiceBusClient(Settings.GetConnectionString());
    var sender = client.CreateSender(queueName);   
    AnsiConsole.Markup("[lime]Sending control message...[/]");
    await sender.SendMessageAsync(message);
    AnsiConsole.MarkupLine("[lime]Done![/]");
    AnsiConsole.WriteLine();
    await sender.CloseAsync();
}

static async Task SendPizzaOrderAsync()
{
    AnsiConsole.MarkupLine("[cyan]SendPizzaOrderAsync[/]");

    var order = new PizzaOrder()
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

    await using var client = new ServiceBusClient(Settings.GetConnectionString(Assembly.GetExecutingAssembly()));
    var sender = client.CreateSender(queueName);
    AnsiConsole.Markup("[lime]Sending order...[/]");
    await sender.SendMessageAsync(message);
    AnsiConsole.MarkupLine("[lime]Done![/]");
    AnsiConsole.WriteLine();
    await sender.CloseAsync();
}

static async Task SendPizzaOrderListAsMessagesAsync()
{
    AnsiConsole.MarkupLine("[cyan]SendPizzaOrderListAsMessagesAsync[/]");

    var pizzaOrderList = GetPizzaOrderList();
    await using var client = new ServiceBusClient(Settings.GetConnectionString());
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
        await sender.SendMessageAsync(message);
    }
    await sender.CloseAsync();
    AnsiConsole.MarkupLineInterpolated($"[lime]Sent {pizzaOrderList.Count} orders! - Time: {watch.ElapsedMilliseconds} milliseconds, that's {pizzaOrderList.Count / watch.Elapsed.TotalSeconds} messages per second.[/]");

    AnsiConsole.WriteLine();
    AnsiConsole.WriteLine();
}

static async Task SendPizzaOrderListAsBatchAsync()
{
    AnsiConsole.MarkupLine("[cyan]SendPizzaOrderListAsBatchAsync[/]");

    var pizzaOrderList = GetPizzaOrderList();
    await using var client = new ServiceBusClient(Settings.GetConnectionString());
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
    await sender.SendMessagesAsync(messageList);
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
