// Create request and response queue clients

using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommonServiceBusConnectionString;

string connectionString = Settings.GetConnectionString();
string requestQueueName = "requestQueue";
string responseQueueName = "responseQueue";

var serviceBusClient = new ServiceBusClient(connectionString);

var responseQueueClient = serviceBusClient.CreateSender(responseQueueName);
var requestQueueClient = serviceBusClient.CreateProcessor(requestQueueName);

Console.WriteLine("Server Console");

// Create a new management client
var managementClient = new ServiceBusAdministrationClient(connectionString);

Console.Write("Creating queues...");

// Delete any existing queues
if (await managementClient.QueueExistsAsync(requestQueueName))
{
    await managementClient.DeleteQueueAsync(requestQueueName);
}

if (await managementClient.QueueExistsAsync(responseQueueName))
{
    await managementClient.DeleteQueueAsync(responseQueueName);
}

// Create Request Queue
var requestQueueResponse = await managementClient.CreateQueueAsync(requestQueueName);
Console.WriteLine($"Request queue created. {requestQueueResponse.Value}" );

// Create Response With Sessions 
var createQueueOptions = new CreateQueueOptions(responseQueueName)
{
    RequiresSession = true
};
var responseQueueResponse = await managementClient.CreateQueueAsync(createQueueOptions);
Console.WriteLine($"Response queue created {responseQueueResponse.Value}");

requestQueueClient.ProcessMessageAsync += ProcessRequestMessage;
requestQueueClient.ProcessErrorAsync += ProcessMessageException;

await requestQueueClient.StartProcessingAsync();

Console.WriteLine("Processing, hit Enter to exit.");
Console.ReadLine();

await requestQueueClient.StopProcessingAsync();

await requestQueueClient.CloseAsync();
await responseQueueClient.CloseAsync();

async Task ProcessRequestMessage(ProcessMessageEventArgs requestMessage)
{
    // Deserialize the message body into text.
    string text =  Encoding.UTF8.GetString(requestMessage.Message.Body);
    Console.WriteLine("Received: " + text);

    Thread.Sleep(DateTime.Now.Millisecond * 20);

    string echoText = "Echo: " + text;
    var responseMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(echoText))
    {
        SessionId = requestMessage.Message.ReplyToSessionId
    };

    // Send the response message.
    await responseQueueClient.SendMessageAsync(responseMessage);
    Console.WriteLine("Sent: " + echoText);
}

async Task ProcessMessageException(ProcessErrorEventArgs arg)
{
    throw arg.Exception;
}

