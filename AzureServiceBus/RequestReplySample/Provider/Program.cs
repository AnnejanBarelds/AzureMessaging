using MessagingUtils;
using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Provider
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Listening for messages...");
            StartListening();
            Console.ReadKey();
        }

        private static void StartListening()
        {
            var settings = new ASBSettings();
            var builder = new ServiceBusConnectionStringBuilder(settings[ASBSettings.ServicebusFqdnEndpoint], ASBSettings.BasicQueueName, ASBSettings.ReceiveKeyName, settings[ASBSettings.ServicebusListenKey]);
            var client = new QueueClient(builder);

            var handlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                AutoComplete = true,
            };

            client.RegisterMessageHandler((message, token) => ProcessRequest(message, token, client), handlerOptions);
        }

        private static async Task ProcessRequest(Message message, CancellationToken token, IQueueClient client)
        {
            var requestBody = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"Message received: {requestBody}");
            var responseBody = Encoding.UTF8.GetBytes($"Reply to '{requestBody}'");
            var reply = new Message(responseBody)
            {
                SessionId = message.ReplyToSessionId
            };
            var builder = new ServiceBusConnectionStringBuilder(message.ReplyTo);
            var replyClient = new QueueClient(builder);
            await replyClient.SendAsync(reply);
            Console.WriteLine("Reply sent");
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
