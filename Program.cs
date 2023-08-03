// See https://aka.ms/new-console-template for more information
using Azure.Messaging.ServiceBus;

//ADD YOUR SB connection string here
//string connectionString="XXX REMOVED";

// CHANGE THESE TO MATCH YOUR KEYVAULT
const string KEY_VAULT = "Rick-KeyVault";
const string SECRET_NAME = "connection-ServiceBus";
var kvURI = "https://" + KEY_VAULT + ".vault.azure.net";
var clientKeyV = new SecretClient(new Uri(kvURI), new DefaultAzureCredential());

// set the key value dynamically
//await client.SetSecretAsync(SECRET_NAME, dynamicconnectionString );
// connection string to your Service Bus namespace  
Console.WriteLine($"Retrieving your secret from {KEY_VAULT}.");
var connectionString = await clientKeyV.GetSecretAsync(SECRET_NAME);


string queueName="ridedata";

// the client that owns the connection and can be used to create senders and receivers
ServiceBusClient client;

ServiceBusProcessor processor;
client = new ServiceBusClient(connectionString);

ServiceBusProcessorOptions processOptions = new ServiceBusProcessorOptions();
processOptions.ReceiveMode = ServiceBusReceiveMode.PeekLock;


// create a processor that we can use to process the messages
processor = client.CreateProcessor(queueName, processOptions);

try
{
    // add handler to process messages
    processor.ProcessMessageAsync += MessageHandler;

    // add handler to process any errors
    processor.ProcessErrorAsync += ErrorHandler;

    // start processing 
    await processor.StartProcessingAsync();

    Console.WriteLine("Wait for a minute and then press any key to end the processing");
    Console.Read();

    // stop processing 
    Console.WriteLine("\nStopping the receiver...");
    await processor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await processor.DisposeAsync();
    await client.DisposeAsync();
}

// handle received messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body}");

    // complete the message. messages is deleted from the queue. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}