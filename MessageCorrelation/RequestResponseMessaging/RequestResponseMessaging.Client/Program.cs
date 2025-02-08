using System.Text;
using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using CommonServiceBusConnectionString;

var connectionString = Settings.GetConnectionString();
var requestQueueName = "requestQueue";
var responseQueueName = "responseQueue";

await  using var client = new ServiceBusClient(connectionString);
await using var requestQueueClient = client.CreateSender(requestQueueName);
//await using var responseQueueClient = client.CreateSessionProcessor(responseQueueName);

Console.WriteLine("Client Console");

while (true)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Enter text:");
    var text = Console.ReadLine() ?? "";

    // Create a session identifier for the response message
    var responseSessionId = Guid.NewGuid().ToString();

    var requestMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(text))
    {
        ReplyToSessionId = responseSessionId
    };

    var stopwatch = Stopwatch.StartNew();

    await requestQueueClient.SendMessageAsync(requestMessage);

    // Accept a message session
    var messageSession = await client.AcceptSessionAsync(responseQueueName, responseSessionId);

    // Receive the response message.
    var responseMessage = await messageSession.ReceiveMessageAsync();
    stopwatch.Stop();

    string echoText = Encoding.UTF8.GetString(responseMessage.Body);

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(echoText);
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Time: {0} ms.", stopwatch.ElapsedMilliseconds);
    Console.WriteLine();
}


