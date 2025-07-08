using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;
using Newtonsoft.Json;
using RfidCheckout.Messages;
using System.Reflection;
using System.Text;
using Spectre.Console;

AnsiConsole.MarkupLine("[bold]Tag Reader Console (sends messages)[/]");

var connectionString = Settings.GetConnectionString(Assembly.GetExecutingAssembly());
const string queueName = "rfidcheckout";

var client = new ServiceBusClient(connectionString);
var sender = client.CreateSender(queueName);

var orderItems = new RfidTag[]
{
        new("Ball", 4.99),
        new("Whistle", 1.95),
        new("Bat", 12.99),
        new("Bat", 12.99),
        new("Gloves", 7.99),
        new("Gloves", 7.99),
        new("Cap", 9.99),
        new("Cap", 9.99),
        new("Shirt", 14.99),
        new("Shirt",  14.99),
};

// Display the order data.
AnsiConsole.MarkupLine("[green]Order contains {0} items.[/]", orderItems.Length);

var orderTotal = 0.0;
foreach (var tag in orderItems)
{
    AnsiConsole.MarkupLine("{0} - ${1}", tag.Product, tag.Price);
    orderTotal += tag.Price;
}
AnsiConsole.MarkupLine("[green]Order value = ${0}.[/]", orderTotal);

AnsiConsole.WriteLine("Press enter to scan...");
Console.ReadLine();

var random = new Random(DateTime.Now.Millisecond);

// Send the order with random duplicate tag reads
var sentCount = 0;
var position = 0;

AnsiConsole.MarkupLine("Reading tags...");

// Comment in to create session id
//var sessionId = Guid.NewGuid().ToString();
//WriteLine($"SessionId: { sessionId }");

while (position < 10)
{
    var rfidTag = orderItems[position];

    // Create a new  message from the order item RFID tag.
    var orderJson = JsonConvert.SerializeObject(rfidTag);
    var tagReadMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(orderJson));

    // Comment in to set message id.
    //tagReadMessage.MessageId = rfidTag.TagId;

    // Comment in to set session id.
    //tagReadMessage.SessionId = sessionId;

    // Send the message
    await sender.SendMessageAsync(tagReadMessage);
    Console.WriteLine($"Sent: { orderItems[position].Product }");
    //WriteLine($"Sent: { orderItems[position].Product } - MessageId: { tagReadMessage.MessageId }");

    // Randomly cause a duplicate message to be sent.
    if (random.NextDouble() > 0.4) position++;
    sentCount++;

    Thread.Sleep(100);
}

AnsiConsole.MarkupLine("[green]{0} total tag reads.[/]", sentCount);
Console.ReadLine();
