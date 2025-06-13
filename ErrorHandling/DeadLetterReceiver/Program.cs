using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;
using Microsoft.VisualBasic.CompilerServices;
using Spectre.Console;

Thread.Sleep(3000);

var queueClient = new ServiceBusClient(Settings.GetConnectionString());
var queueName = "errorhandling";

AnsiConsole.MarkupLine("[bold white]DeadLetterReceiverConsole[/]");

var options = new ServiceBusProcessorOptions
{
    MaxConcurrentCalls = 1,
    AutoCompleteMessages = false,
    SubQueue = SubQueue.DeadLetter
};

var processor =  queueClient.CreateProcessor(queueName, options);

Utils.WriteLine($"Dead letter path: {processor.EntityPath}", ConsoleColor.Cyan);

processor.ProcessMessageAsync += ProcessDeadLetterMessage;
processor.ProcessErrorAsync += ProcessError;

await processor.StartProcessingAsync();

Utils.WriteLine("Receiving dead letter messages", ConsoleColor.Cyan);
Console.WriteLine();

Console.ReadLine();
await processor.StopProcessingAsync();
await processor.CloseAsync();

async Task ProcessDeadLetterMessage(ProcessMessageEventArgs message)
{
    Utils.WriteLine("Received dead letter message", ConsoleColor.Cyan);
    Utils.WriteLine($"    Content type: { message.Message.ContentType }", ConsoleColor.Green);
    Utils.WriteLine($"    DeadLetterReason: { message.Message.DeadLetterReason }", ConsoleColor.Green);
    Utils.WriteLine($"    DeadLetterErrorDescription: { message.Message.DeadLetterErrorDescription }", ConsoleColor.Green);

    await message.CompleteMessageAsync(message.Message);
    Console.WriteLine();
}

Task ProcessError(ProcessErrorEventArgs arg)
{
    AnsiConsole.WriteException(arg.Exception);
    return Task.CompletedTask;
}