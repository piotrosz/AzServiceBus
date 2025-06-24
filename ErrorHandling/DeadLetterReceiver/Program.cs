using System.Reflection;
using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;
using Spectre.Console;

await Task.Delay(TimeSpan.FromSeconds(3));

var queueClient = new ServiceBusClient(Settings.GetConnectionString(Assembly.GetExecutingAssembly()));
const string queueName = "errorhandling";

AnsiConsole.Write(new FigletText("Dead letter Receiver Console").Color(Color.Red1));
AnsiConsole.WriteLine();

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