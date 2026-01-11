using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;
using Spectre.Console;
using System.Diagnostics;
using System.Reflection;
using System.Text;

var connectionString = Settings.GetConnectionString(Assembly.GetExecutingAssembly());
const string requestQueueName = "requestQueue";
const string responseQueueName = "responseQueue";

await  using var client = new ServiceBusClient(connectionString);
await using var requestQueueClient = client.CreateSender(requestQueueName);
//await using var responseQueueClient = client.CreateSessionProcessor(responseQueueName);

AnsiConsole.Write(new FigletText("Client Console").Color(Color.Blue));

while (true)
{
    AnsiConsole.MarkupLine("[white]Enter text:[/]");
    var text = AnsiConsole.Ask<string>("[grey]>[/]");

    // Create a session identifier for the response message
    var responseSessionId = Guid.NewGuid().ToString();

    var requestMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(text))
    {
        ReplyToSessionId = responseSessionId
    };

    var stopwatch = Stopwatch.StartNew();

    await requestQueueClient.SendMessageAsync(requestMessage);

    // Accept a message session
    var messageSession = await client.AcceptSessionAsync(responseQueueName, responseSessionId);

    // Receive the response message.
    var responseMessage = await messageSession.ReceiveMessageAsync();
    stopwatch.Stop();    
    var echoText = Encoding.UTF8.GetString(responseMessage.Body);

    AnsiConsole.MarkupLine($"[yellow]{echoText.EscapeMarkup()}[/]");
    AnsiConsole.MarkupLine($"[white]Time: {stopwatch.ElapsedMilliseconds} ms.[/]");
    AnsiConsole.WriteLine();
}


