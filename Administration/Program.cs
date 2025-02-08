using AzServiceBusAdministration;
using CommonServiceBusConnectionString;
using Microsoft.Extensions.Configuration;

// Enter a valid Service Bus connection string
// Minimum "Stardard" tier to create topics

var serviceBusConnectionString = Settings.GetConnectionString();
 var helper = new ManagementHelper(serviceBusConnectionString);

var done = false;
do
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write(">");
    var commandLine = Console.ReadLine();
    Console.ForegroundColor = ConsoleColor.Magenta;
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
                        helper.CreateQueueAsync(commands[1]).Wait();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Queue path not specified.");
                    }
                    break;
                case "listqueues" or "lq" :
                    helper.ListQueuesAsync().Wait();
                    break;
                case "getqueue" or "gq" :
                    if (commands.Length > 1)
                    {
                        helper.GetQueueAsync(commands[1]).Wait();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Queue path not specified.");
                    }
                    break;
                case "deletequeue" or "dq" :
                    if (commands.Length > 1)
                    {
                        helper.DeleteQueueAsync(commands[1]).Wait();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Queue path not specified.");
                    }
                    break;
                case "createtopic" or "ct":
                    if (commands.Length > 1)
                    {
                        helper.CreateTopicAsync(commands[1]).Wait();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Topic path not specified.");
                    }
                    break;
                case "createsubscription" or "cs":
                    if (commands.Length > 2)
                    {
                        helper.CreateSubscriptionAsync(commands[1], commands[2]).Wait();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Topic path not specified.");
                    }
                    break;
                case "listtopics" or "lt":
                    helper.ListTopicsAndSubscriptionsAsync().Wait();
                    break;
                case "exit":
                    done = true;
                    break;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
    }
} while (!done);
        

