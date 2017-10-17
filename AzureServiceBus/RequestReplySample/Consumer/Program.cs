using MessagingUtils;
using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sending out a request message...");
            var handler = new RequestReplyHandler();
            var message = new Message(Encoding.UTF8.GetBytes("Some random content"));
            string content = null;
            handler.Request(message, TimeSpan.FromSeconds(30), reply =>
            {
                content = Encoding.UTF8.GetString(reply.Body);
                Console.WriteLine($"Received an answer: {content}");
                return Task.FromResult(true);
            }, true).Wait();
            Console.ReadKey();
        }
    }
}
