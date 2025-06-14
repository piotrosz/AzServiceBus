using AzServiceBusAdministration;
using CommonServiceBusConnectionString;
using Spectre.Console;

// Minimum "Stardard" tier to create topics

var serviceBusConnectionString = Settings.GetConnectionString();
 var helper = new ManagementHelper(serviceBusConnectionString);

var done = false;
do
{
    AnsiConsole.MarkupLine("[cyan]>[/]");
    var commandLine = Console.ReadLine();
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
        

