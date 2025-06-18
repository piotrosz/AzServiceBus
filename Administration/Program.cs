using AzServiceBusAdministration;
using CommonServiceBusConnectionString;
using Spectre.Console;

// Minimum "Stardard" tier to create topics

var serviceBusConnectionString = Settings.GetConnectionString();
var helper = new ManagementHelper(serviceBusConnectionString);

DisplayHelp();

var done = false;
do
{
    var commandLine = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]>[/]"));
    var commands = commandLine.Split(' ');

    try
    {
        if (commands.Length > 0)
        {
            switch (commands[0])
            {
                case Commands.CreateQueue:
                case Commands.CreateQueueShort:
                    if (commands.Length > 1)
                    {
                        await helper.CreateQueueAsync(commands[1]);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Queue path not specified.[/]");
                    }
                    break;
                case Commands.ListQueues:
                case Commands.ListQueuesShort:
                    await helper.ListQueuesAsync();
                    break;
                case Commands.GetQueue:
                case Commands.GetQueueShort:
                    if (commands.Length > 1)
                    {
                        await helper.GetQueueAsync(commands[1]);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Queue path not specified.[/]");
                    }
                    break;
                case Commands.DeleteQueue:
                case Commands.DeleteQueueShort:
                    if (commands.Length > 1)
                    {
                        await helper.DeleteQueueAsync(commands[1]);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Queue path not specified.[/]");
                    }
                    break;
                case Commands.CreateTopic:
                case Commands.CreateTopicShort:
                    if (commands.Length > 1)
                    {
                        await helper.CreateTopicAsync(commands[1]);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Topic path not specified.[/]");
                    }
                    break;
                case Commands.CreateSubscription:
                case Commands.CreateSubscriptionShort:
                    if (commands.Length > 2)
                    {
                        await helper.CreateSubscriptionAsync(commands[1], commands[2]);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Topic path not specified.[/]");
                    }
                    break;
                case Commands.ListTopics:
                case Commands.ListTopicsShort:
                    await helper.ListTopicsAndSubscriptionsAsync();
                    break;
                case Commands.Help:
                    DisplayHelp();
                    break;
                case Commands.Exit:
                    done = true;
                    break;
            }
        }
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
    }
} while (!done);

static void DisplayHelp()
{    var panel = new Panel(BuildHelpContent())
    {
        Border = BoxBorder.Rounded,
        Header = new PanelHeader("[green]Available Commands[/]"),
        Padding = new Padding(1, 0, 1, 0)
    };
    
    AnsiConsole.Write(panel);
}

static Table BuildHelpContent()
{
    var table = new Table()
        .HideHeaders()
        .Border(TableBorder.None)
        .AddColumn(new TableColumn("Command").Width(25))
        .AddColumn(new TableColumn("Description").Width(50));
      table.AddRow($"[cyan]{Commands.CreateQueue}[/] or [cyan]{Commands.CreateQueueShort}[/] <queue>", "Create a new queue");
    table.AddRow($"[cyan]{Commands.ListQueues}[/] or [cyan]{Commands.ListQueuesShort}[/]", "List all queues");
    table.AddRow($"[cyan]{Commands.GetQueue}[/] or [cyan]{Commands.GetQueueShort}[/] <queue>", "Get details of a queue");
    table.AddRow($"[cyan]{Commands.DeleteQueue}[/] or [cyan]{Commands.DeleteQueueShort}[/] <queue>", "Delete a queue");
    table.AddRow($"[cyan]{Commands.CreateTopic}[/] or [cyan]{Commands.CreateTopicShort}[/] <topic>", "Create a new topic");
    table.AddRow($"[cyan]{Commands.CreateSubscription}[/] or [cyan]{Commands.CreateSubscriptionShort}[/] <topic> <subscription>", "Create a subscription for a topic");
    table.AddRow($"[cyan]{Commands.ListTopics}[/] or [cyan]{Commands.ListTopicsShort}[/]", "List all topics and subscriptions");
    table.AddRow($"[cyan]{Commands.Help}[/]", "Show this help panel");
    table.AddRow($"[cyan]{Commands.Exit}[/]", "Exit the application");
    
    return table;
}


