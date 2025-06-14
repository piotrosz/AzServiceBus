using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;
using Microsoft.VisualBasic.CompilerServices;
using Spectre.Console;

Thread.Sleep(3000);

var queueClient = new ServiceBusClient(Settings.GetConnectionString());
const string queueName = "errorhandling";

AnsiConsole.MarkupLine("[bold white]DeadLetterReceiverConsole[/]");

var options = new ServiceBusProcessorOptions
{
    MaxConcurrentCalls = 1,
    AutoCompleteMessages = false,
    SubQueue = SubQueue.DeadLetter
};

var processor =  queueClient.CreateProcessor(queueName, options);

AnsiConsole.MarkupLine($"[cyan]Dead letter path: {processor.EntityPath}[/]");

processor.ProcessMessageAsync += ProcessDeadLetterMessage;
processor.ProcessErrorAsync += ProcessError;

await processor.StartProcessingAsync();

AnsiConsole.MarkupLine("[cyan]Receiving dead letter messages[/]");
Console.WriteLine();
Console.ReadLine();

await processor.StopProcessingAsync();
await processor.CloseAsync();

return;

async Task ProcessDeadLetterMessage(ProcessMessageEventArgs message)
{
    AnsiConsole.MarkupLine("[cyan]Received dead letter message[/]");
    
    AnsiConsole.MarkupLine($"[green]Content type: { message.Message.ContentType }[/]");
    AnsiConsole.MarkupLine($"[green]DeadLetterReason: { message.Message.DeadLetterReason }[/]");
    AnsiConsole.MarkupLine($"[green]DeadLetterErrorDescription: { message.Message.DeadLetterErrorDescription }[/]");

    await message.CompleteMessageAsync(message.Message);
    Console.WriteLine();
}

Task ProcessError(ProcessErrorEventArgs arg)
{
    AnsiConsole.WriteException(arg.Exception);
    return Task.CompletedTask;
}