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
                case "createqueue" or "cq" :
                    if (commands.Length > 1)
                    {
                        await helper.CreateQueueAsync(commands[1]);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Queue path not specified.[/]");
                    }
                    break;
                case "listqueues" or "lq" :
                    await helper.ListQueuesAsync();
                    break;
                case "getqueue" or "gq" :
                    if (commands.Length > 1)
                    {
                        await helper.GetQueueAsync(commands[1]);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Queue path not specified.[/]");
                    }
                    break;
                case "deletequeue" or "dq" :
                    if (commands.Length > 1)
                    {
                        await helper.DeleteQueueAsync(commands[1]);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Queue path not specified.[/]");
                    }
                    break;
                case "createtopic" or "ct":
                    if (commands.Length > 1)
                    {
                        await helper.CreateTopicAsync(commands[1]);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Topic path not specified.[/]");
                    }
                    break;
                case "createsubscription" or "cs":
                    if (commands.Length > 2)
                    {
                        await helper.CreateSubscriptionAsync(commands[1], commands[2]);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Topic path not specified.[/]");
                    }
                    break;
                case "listtopics" or "lt":
                    await helper.ListTopicsAndSubscriptionsAsync();
                    break;
                case "help":
                    DisplayHelp();
                    break;
                case "exit":
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
    
    table.AddRow("[cyan]createqueue[/] or [cyan]cq[/] <queue>", "Create a new queue");
    table.AddRow("[cyan]listqueues[/] or [cyan]lq[/]", "List all queues");
    table.AddRow("[cyan]getqueue[/] or [cyan]gq[/] <queue>", "Get details of a queue");
    table.AddRow("[cyan]deletequeue[/] or [cyan]dq[/] <queue>", "Delete a queue");
    table.AddRow("[cyan]createtopic[/] or [cyan]ct[/] <topic>", "Create a new topic");
    table.AddRow("[cyan]createsubscription[/] or [cyan]cs[/] <topic> <subscription>", "Create a subscription for a topic");
    table.AddRow("[cyan]listtopics[/] or [cyan]lt[/]", "List all topics and subscriptions");
    table.AddRow("[cyan]help[/]", "Show this help panel");
    table.AddRow("[cyan]exit[/]", "Exit the application");
    
    return table;
}


