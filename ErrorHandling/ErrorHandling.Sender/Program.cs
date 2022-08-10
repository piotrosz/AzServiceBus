using System.Text;
using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;
using ErrorHandling.Sender;

await using var client = new ServiceBusClient(Settings.GetConnectionString());
var queueName = "errorhandling";
var sender = client.CreateSender(queueName);

Console.WriteLine("Sender Console");
Console.WriteLine();

Thread.Sleep(3000);

while (true)
{
    Console.WriteLine("text/json/poison/unknown/exit?");

    var messageType = Console.ReadLine().ToLower();

    if (messageType == "exit")
    {
        break;
    }

    switch (messageType)
    {
        case "text":
            await SendMessage("Hello", "text/plain");
            break;
        case "json":
            await SendMessage("{\"contact\": {\"name\": \"Alan\",\"twitter\": \"@alansmith\" }}", "application/json");
            break;
        case "poison":
            // xml in content but contentType set to json
            await SendMessage("<contact><name>Alan</name><twitter>@alansmith</twitter></contact>", "application/json");
            break;
        case "unknown":
            await SendMessage("Unknown message", "application/unknown");
            break;

        default:
            Console.WriteLine("What?");
            break;
    }
}

await sender.CloseAsync();

async Task SendMessage(string text, string contentType)
{
   
    try
    {
        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(text))
        {
            ContentType = contentType
        };
        Utils.WriteLine($"Created Message: { text }", ConsoleColor.Cyan);
     
        await sender.SendMessageAsync(message);
        Utils.WriteLine("Sent Message", ConsoleColor.Cyan);
    }
    catch (Exception ex)
    {
        Utils.WriteLine(ex.Message, ConsoleColor.Yellow);
    }
}
