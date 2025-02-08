using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;
using DeadLetterReceiver;

Thread.Sleep(3000);

var queueClient = new ServiceBusClient(Settings.GetConnectionString());
string queueName = "errorhandling";

Utils.WriteLine("DeadLetterReceiverConsole", ConsoleColor.White);
Console.WriteLine();

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

async Task ProcessError(ProcessErrorEventArgs arg)
{
    Utils.WriteLine($"Exception: { arg.Exception.Message }", ConsoleColor.Yellow);
}