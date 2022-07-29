using System.Text;
using WorkingWithMessages.MessageEntities;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;
using Newtonsoft.Json;

await using ServiceBusClient QueueClient = new ServiceBusClient(Settings.GetConnectionString());
const string QueueName = "workingwithmessages";

WriteLine("Receiver Console", ConsoleColor.White);

await RecreateQueueAsync();

//Comment in the appropriate method

await ReceiveAndProcessText(1);

//await ReceiveAndProcessPizzaOrders(1);
//await ReceiveAndProcessPizzaOrders(5);
//await ReceiveAndProcessPizzaOrders(100);

//await ReceiveAndProcessControlMessage(1);

//await ReceiveAndProcessCharacters(1);

//await ReceiveAndProcessCharacters(16);


async Task ReceiveAndProcessText(int threads)
{
    WriteLine($"ReceiveAndProcessText({ threads })", ConsoleColor.Cyan);
    
    var options = new ServiceBusProcessorOptions
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(30)
    };
    
    var processor =  QueueClient.CreateProcessor(QueueName, options);

    processor.ProcessMessageAsync += ProcessTextMessageAsync;
    processor.ProcessErrorAsync += ProcessErrorHandler;

    Console.WriteLine("Start processing");
    await processor.StartProcessingAsync();
    
    WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
    Console.ReadLine();
    await processor.StopProcessingAsync();
    await processor.CloseAsync();
}

async Task ReceiveAndProcessControlMessage(int threads)
{
    WriteLine($"ReceiveAndProcessPizzaOrders({ threads })", ConsoleColor.Cyan);
    
    var options = new ServiceBusProcessorOptions
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(30)
    };
    
    var processor = QueueClient.CreateProcessor(QueueName, options);

    processor.ProcessMessageAsync += ProcessControlMessageAsync;
    processor.ProcessErrorAsync += ProcessErrorHandler;
    
    WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
    Console.ReadLine();
    await processor.CloseAsync();
}

async Task ReceiveAndProcessPizzaOrders(int threads)
{
    WriteLine($"ReceiveAndProcessPizzaOrders({ threads })", ConsoleColor.Cyan);
    
    var options = new ServiceBusProcessorOptions()
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10)
    };

    var processor = QueueClient.CreateProcessor(QueueName, options);
    
    processor.ProcessMessageAsync += ProcessPizzaMessageAsync;
    processor.ProcessErrorAsync += ProcessErrorHandler;
    await processor.StartProcessingAsync();

    WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
    Console.ReadLine();
    await processor.StopProcessingAsync();
    await processor.CloseAsync();
}

async Task ProcessPizzaMessageAsync(ProcessMessageEventArgs message)
{
    var messageBodyText = Encoding.UTF8.GetString(message.Message.Body);

    var pizzaOrder = JsonConvert.DeserializeObject<PizzaOrder>(messageBodyText);

    CookPizza(pizzaOrder);

    await message.CompleteMessageAsync(message.Message);

}

async Task ProcessTextMessageAsync(ProcessMessageEventArgs message)
{
    var messageBodyText = Encoding.UTF8.GetString(message.Message.Body);

    WriteLine($"Received: { messageBodyText }", ConsoleColor.Green);

    await message.CompleteMessageAsync(message.Message);
}

async Task ProcessControlMessageAsync(ProcessMessageEventArgs message)
{
    WriteLine($"Received: { message.Message.Subject }", ConsoleColor.Green);

    WriteLine("User properties...", ConsoleColor.Yellow);
    foreach (var property in message.Message.ApplicationProperties)
    {
        WriteLine($"    { property.Key } - { property.Value }", ConsoleColor.Cyan);
    }

    await message.CompleteMessageAsync(message.Message);
}

Task ProcessErrorHandler(ProcessErrorEventArgs exceptionReceivedEventArgs)
{
    WriteLine(exceptionReceivedEventArgs.Exception.Message, ConsoleColor.Red);
    return Task.CompletedTask;
}

async Task ReceiveAndProcessCharacters(int threads)
{
    WriteLine($"ReceiveAndProcessCharacters({ threads })", ConsoleColor.Cyan);
    
    var options = new ServiceBusProcessorOptions
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = threads,
        MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(30)
    };

    var processor= QueueClient.CreateProcessor(QueueName, options);

    processor.ProcessMessageAsync += ProcessCharacterMessageAsync;
    processor.ProcessErrorAsync += ProcessErrorHandler;
    await processor.StartProcessingAsync();

    WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
    Console.ReadLine();
    await processor.StopProcessingAsync();
    await processor.CloseAsync();
}

async Task ProcessCharacterMessageAsync(ProcessMessageEventArgs message)
{
    Write(message.Message.Subject, ConsoleColor.Green);
    await message.CompleteMessageAsync(message.Message);
}

static async Task RecreateQueueAsync()
{
    var manager = new ServiceBusAdministrationClient(Settings.GetConnectionString());
    if (await manager.QueueExistsAsync(QueueName))
    {
        WriteLine($"Deleting queue: { QueueName }...", ConsoleColor.Magenta);
        await manager.DeleteQueueAsync(QueueName);
        WriteLine("Done!", ConsoleColor.Magenta);
    }

    WriteLine($"Creating queue: { QueueName }...", ConsoleColor.Magenta);
    await manager.CreateQueueAsync(QueueName);
    WriteLine("Done!", ConsoleColor.Magenta);
}

static void CookPizza(PizzaOrder order)
{
    WriteLine($"Cooking {  order.Type } for { order.CustomerName }.", ConsoleColor.Yellow);
    Thread.Sleep(5000);
    WriteLine($"    { order.Type } pizza for {  order.CustomerName } is ready!", ConsoleColor.Green);
}

static void WriteLine(string text, ConsoleColor color)
{
    var tempColor = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.WriteLine(text);
    Console.ForegroundColor = tempColor;
}

static void Write(string text, ConsoleColor color)
{
    var tempColor = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.Write(text);
    Console.ForegroundColor = tempColor;
}
    
