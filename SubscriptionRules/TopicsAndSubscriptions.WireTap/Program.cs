using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;
using Spectre.Console;
using static System.Console;

var serviceBusConnectionString = Settings.GetConnectionString();
const string topicName = "Orders";

AnsiConsole.MarkupLine("[bold blue]Wire Tap Console[/]");
AnsiConsole.MarkupLine("[yellow]Press enter to activate wire tap[/]");
ReadLine();

var subscriptionName = $"wiretap-{ Guid.NewGuid() }";

var managementClient = new ServiceBusAdministrationClient(serviceBusConnectionString);

var options = new CreateSubscriptionOptions(topicName, subscriptionName)
{
    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
};
var subscription = await managementClient.CreateSubscriptionAsync(options);

await using var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
await using var receiver = serviceBusClient.CreateReceiver(topicName, subscriptionName);

AnsiConsole.MarkupLine($"[green]Receiving on[/] [bold green]{ subscriptionName }[/]");
AnsiConsole.MarkupLine("[yellow]Press enter to quit...[/]");

// Set up cancellation token source for graceful exit
using var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

// Start a task to listen for console input to exit
var exitTask = Task.Run(() => 
{
    ReadLine();
    cts.Cancel();
});

try
{
    while (!cancellationToken.IsCancellationRequested)
    { 
        var message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(1), cancellationToken);
        if (message != null)
        {
            InspectMessage(message);
            await receiver.CompleteMessageAsync(message);
        }
    }
}
catch (OperationCanceledException)
{
    // Graceful exit
}
finally
{
    await receiver.CloseAsync();
    AnsiConsole.MarkupLine("[bold green]Wire Tap closed.[/]");
}

void InspectMessage(ServiceBusReceivedMessage message)
{
    AnsiConsole.MarkupLine($"[bold green]Received message...[/]");
    
    // Create a table for message properties
    var propertiesTable = new Table().Border(TableBorder.Rounded);
    propertiesTable.Title = new TableTitle("[bold yellow]Message Properties[/]");
    
    // Add columns
    propertiesTable.AddColumn(new TableColumn("Property").Centered());
    propertiesTable.AddColumn(new TableColumn("Value").Centered());
    
    // Add rows for each property
    propertiesTable.AddRow("ContentType", message.ContentType ?? "null");
    propertiesTable.AddRow("CorrelationId", message.CorrelationId ?? "null");
    propertiesTable.AddRow("Subject", message.Subject ?? "null");
    propertiesTable.AddRow("MessageId", message.MessageId ?? "null");
    propertiesTable.AddRow("PartitionKey", message.PartitionKey ?? "null");
    propertiesTable.AddRow("ReplyTo", message.ReplyTo ?? "null");
    propertiesTable.AddRow("ReplyToSessionId", message.ReplyToSessionId ?? "null");
    propertiesTable.AddRow("ScheduledEnqueueTime", message.ScheduledEnqueueTime.ToString());
    propertiesTable.AddRow("SessionId", message.SessionId ?? "null");
    propertiesTable.AddRow("TimeToLive", message.TimeToLive.ToString());
    propertiesTable.AddRow("To", message.To ?? "null");
    
    // Render the properties table
    AnsiConsole.Write(propertiesTable);
    
    // Create a table for application properties if any exist
    if (message.ApplicationProperties.Any())
    {
        var appPropsTable = new Table().Border(TableBorder.Rounded);
        appPropsTable.Title = new TableTitle("[bold yellow]Application Properties[/]");
        
        // Add columns
        appPropsTable.AddColumn(new TableColumn("Key").Centered());
        appPropsTable.AddColumn(new TableColumn("Value").Centered());
        
        // Add rows for each application property
        foreach (var property in message.ApplicationProperties)
        {
            appPropsTable.AddRow(property.Key, property.Value?.ToString() ?? "null");
        }
        
        // Render the application properties table
        AnsiConsole.Write(appPropsTable);
    }
    
    // Display the message body in a panel
    var bodyPanel = new Panel(Encoding.UTF8.GetString(message.Body))
    {
        Header = new PanelHeader("[bold yellow]Message Body[/]"),
        Border = BoxBorder.Rounded,
        Expand = true,
        Padding = new Padding(1)
    };
    
    AnsiConsole.Write(bodyPanel);
    AnsiConsole.WriteLine();
}