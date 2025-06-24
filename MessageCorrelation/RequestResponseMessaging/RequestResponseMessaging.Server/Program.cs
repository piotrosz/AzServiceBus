// Create request and response queue clients

using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;
using Spectre.Console;
using System.Reflection;
using System.Text;

var connectionString = Settings.GetConnectionString(Assembly.GetExecutingAssembly());
const string requestQueueName = "requestQueue";
const string responseQueueName = "responseQueue";

var serviceBusClient = new ServiceBusClient(connectionString);

var responseQueueClient = serviceBusClient.CreateSender(responseQueueName);
var requestQueueClient = serviceBusClient.CreateProcessor(requestQueueName);

AnsiConsole.MarkupLine("[bold green]Server Console[/]");

// Create a new management client
var managementClient = new ServiceBusAdministrationClient(connectionString);

AnsiConsole.Markup("[yellow]Creating queues...[/]");

// Delete any existing queues
if (await managementClient.QueueExistsAsync(requestQueueName))
{
    await managementClient.DeleteQueueAsync(requestQueueName);
}

if (await managementClient.QueueExistsAsync(responseQueueName))
{
    await managementClient.DeleteQueueAsync(responseQueueName);
}

// Create Request Queue
var requestQueueResponse = await managementClient.CreateQueueAsync(requestQueueName);
AnsiConsole.MarkupLine($"[green]Request queue created.[/] {requestQueueResponse.Value}" );

// Create Response With Sessions 
var createQueueOptions = new CreateQueueOptions(responseQueueName)
{
    RequiresSession = true
};
var responseQueueResponse = await managementClient.CreateQueueAsync(createQueueOptions);
AnsiConsole.MarkupLine($"[green]Response queue created[/] {responseQueueResponse.Value}");

requestQueueClient.ProcessMessageAsync += ProcessRequestMessage;
requestQueueClient.ProcessErrorAsync += ProcessMessageException;

await requestQueueClient.StartProcessingAsync();

AnsiConsole.MarkupLine("[blue]Processing, hit Enter to exit.[/]");
Console.ReadLine();

await requestQueueClient.StopProcessingAsync();

await requestQueueClient.CloseAsync();
await responseQueueClient.CloseAsync();

return;

async Task ProcessRequestMessage(ProcessMessageEventArgs requestMessage)
{
    // Deserialize the message body into text.
    var text =  Encoding.UTF8.GetString(requestMessage.Message.Body);
    AnsiConsole.MarkupLine($"[cyan]Received:[/] {text.EscapeMarkup()}");

    Thread.Sleep(DateTime.Now.Millisecond * 20);

    var echoText = $"Echo: {text}";
    var responseMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(echoText))
    {
        SessionId = requestMessage.Message.ReplyToSessionId
    };

    // Send the response message.
    await responseQueueClient.SendMessageAsync(responseMessage);
    AnsiConsole.MarkupLine($"[green]Sent:[/] {echoText.EscapeMarkup()}");
}

Task ProcessMessageException(ProcessErrorEventArgs arg)
{
    throw arg.Exception;
}

