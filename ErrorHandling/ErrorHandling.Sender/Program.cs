using System.Text;
using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;
using Spectre.Console;

await using var client = new ServiceBusClient(Settings.GetConnectionString());
var queueName = "errorhandling";
var sender = client.CreateSender(queueName);

AnsiConsole.Write(new FigletText("Sender Console").Color(Color.Green));
AnsiConsole.WriteLine();

Thread.Sleep(3000);

while (true)
{
    AnsiConsole.MarkupLine("[blue]text/json/poison/unknown/exit?[/]");

    var messageType = AnsiConsole.Ask<string>("[grey]>[/]").ToLower();

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
            AnsiConsole.MarkupLine("[red]What?[/]");
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
        AnsiConsole.MarkupLine($"[cyan]Created Message: {text.EscapeMarkup()}[/]");

        await sender.SendMessageAsync(message);
        AnsiConsole.MarkupLine("[cyan]Sent Message[/]");
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
    }
}
