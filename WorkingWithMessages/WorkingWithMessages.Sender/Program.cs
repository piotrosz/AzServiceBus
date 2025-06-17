using System.Diagnostics;
using System.Text;
using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;
using Newtonsoft.Json;
using WorkingWithMessages.MessageEntities;

const string queueName = "workingwithmessages";

WriteLine("Sender Console - Hit enter", ConsoleColor.White);
Console.ReadLine();

//ToDo: Comment in the appropriate method

//await SendTextString("The quick brown fox jumps over the lazy dog");

//await SendPizzaOrderAsync();

//await SendControlMessageAsync();

//await SendPizzaOrderListAsMessagesAsync();
        
await SendPizzaOrderListAsBatchAsync();

//await SendTextStringAsMessagesAsync("The quick brown fox jumps over the lazy dog");

//await SendTextStringAsBatchAsync("The quick brown fox jumps over the lazy dog");


WriteLine("Sender Console - Complete", ConsoleColor.White);
Console.ReadLine();

return;

static async Task SendTextString(string text)
{
    WriteLine("SendTextStringAsMessagesAsync", ConsoleColor.Cyan);

    await using var client = new ServiceBusClient(Settings.GetConnectionString());
    var sender = client.CreateSender(queueName);

    Write("Sending...", ConsoleColor.Green);

    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(text));
    await sender.SendMessageAsync(message);

    WriteLine("Done!", ConsoleColor.Green);

    Console.WriteLine();

    await sender.CloseAsync();
}


static async Task SendTextStringAsMessagesAsync(string text)
{
    WriteLine("SendTextStringAsMessagesAsync", ConsoleColor.Cyan);

    // Create a client
    await using var client = new ServiceBusClient(Settings.GetConnectionString());
    var sender = client.CreateSender(queueName);

    Write("Sending:", ConsoleColor.Green);

    foreach (var letter in text.ToCharArray())
    {
        var message = new ServiceBusMessage
        {
            Subject = letter.ToString()
        };

        await sender.SendMessageAsync(message);
        Write(message.MessageId, ConsoleColor.Green);
    }

    Console.WriteLine();
    Console.WriteLine();

    await sender.CloseAsync();
}

static async Task SendTextStringAsBatchAsync(string text)
{
    WriteLine("SendTextStringAsBatchAsync", ConsoleColor.Cyan);

    await using var client = new ServiceBusClient(Settings.GetConnectionString());
    var sender = client.CreateSender(queueName);

    Write("Sending:", ConsoleColor.Green);
    
    var messageList = new List<ServiceBusMessage>();

    foreach (var letter in text.ToCharArray())
    {
        var message = new ServiceBusMessage
        {
            Subject = letter.ToString()
        };

        messageList.Add(message);

    }

    await sender.SendMessagesAsync(messageList);

    Console.WriteLine();
    Console.WriteLine();

    await sender.CloseAsync();
}

static async Task SendControlMessageAsync()
{
    WriteLine("SendControlMessageAsync", ConsoleColor.Cyan);

    var message = new ServiceBusMessage()
    {
        MessageId = "Control"
    };

    message.ApplicationProperties.Add("SystemId", 1462);
    message.ApplicationProperties.Add("Command", "Pending Restart");
    message.ApplicationProperties.Add("ActionTime", DateTime.UtcNow.AddHours(2));

    await using var client = new ServiceBusClient(Settings.GetConnectionString());
    var sender = client.CreateSender(queueName);

    Write("Sending control message...", ConsoleColor.Green);
    await sender.SendMessageAsync(message);
    WriteLine("Done!", ConsoleColor.Green);
    Console.WriteLine();
    await sender.CloseAsync();
}

static async Task SendPizzaOrderAsync()
{
    WriteLine("SendPizzaOrderAsync", ConsoleColor.Cyan);

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

    await using var client = new ServiceBusClient(Settings.GetConnectionString());
    var sender = client.CreateSender(queueName);
    Write("Sending order...", ConsoleColor.Green);
    await sender.SendMessageAsync(message);
    WriteLine("Done!", ConsoleColor.Green);
    Console.WriteLine();
    await sender.CloseAsync();
}

static async Task SendPizzaOrderListAsMessagesAsync()
{
    WriteLine("SendPizzaOrderListAsMessagesAsync", ConsoleColor.Cyan);

    var pizzaOrderList = GetPizzaOrderList();

    await using var client = new ServiceBusClient(Settings.GetConnectionString());
    var sender = client.CreateSender(queueName);

    WriteLine("Sending...", ConsoleColor.Yellow);
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
    WriteLine($"Sent { pizzaOrderList.Count } orders! - Time: { watch.ElapsedMilliseconds } milliseconds, that's { pizzaOrderList.Count / watch.Elapsed.TotalSeconds } messages per second.", ConsoleColor.Green);
    Console.WriteLine();
    Console.WriteLine();
}

static async Task SendPizzaOrderListAsBatchAsync()
{
    WriteLine("SendPizzaOrderListAsBatchAsync", ConsoleColor.Cyan);

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
        messageList.Add(message);
    }

    WriteLine("Sending...", ConsoleColor.Yellow);
    await sender.SendMessagesAsync(messageList);

    await sender.CloseAsync();

    WriteLine($"Sent { pizzaOrderList.Count } orders! - Time: { watch.ElapsedMilliseconds } milliseconds, that's { pizzaOrderList.Count / watch.Elapsed.TotalSeconds } messages per second.", ConsoleColor.Green);
    Console.WriteLine();
    Console.WriteLine();
}

static List<PizzaOrder> GetPizzaOrderList()
{
    // Create some data
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
