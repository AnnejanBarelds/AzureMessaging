using Microsoft.Azure.ServiceBus;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MessagingUtils
{
    public class RequestReplyHandler
    {
        private IQueueClient _sendClient;
        private ISessionClient _receiveClient;
        private ASBSettings _settings;

        private readonly string _receiveKeyName;
        private readonly string _receiveKey;

        public RequestReplyHandler()
        {
            _settings = new ASBSettings();

            /* We could use the samplesend SAS Policy from the above settings for the reply channel as well. But that would mean that the SAS token created for the Provider would be valid for
             * more queues than just the one we want to use as the reply queue. That is undesirable for a Provider that may be outside of our own organisation for example.
             * So we created a new SAS Policy names SessionQueueSend scoped just to the sessionqueue. */
            _receiveKeyName = "SessionQueueSend";
            _receiveKey = "<SessionQueueSend key>";

            var sendBuilder = new ServiceBusConnectionStringBuilder(_settings[ASBSettings.ServicebusFqdnEndpoint], ASBSettings.BasicQueueName, ASBSettings.SendKeyName, _settings[ASBSettings.ServicebusSendKey]);
            var receiveBuilder = new ServiceBusConnectionStringBuilder(_settings[ASBSettings.ServicebusFqdnEndpoint], ASBSettings.SessionQueueName, ASBSettings.ReceiveKeyName, _settings[ASBSettings.ServicebusListenKey]);

            _sendClient = new QueueClient(sendBuilder);
            _receiveClient = new SessionClient(receiveBuilder);
        }

        public async Task Request(Message message, TimeSpan timeout, Func<Message, Task<bool>> replyHandler, bool deadletterOnHandlerFailure = false)
        {
            var sasToken = GenerateSasToken($"{_settings[ASBSettings.ServicebusFqdnEndpoint]}{ASBSettings.SessionQueueName}/", _receiveKeyName, _receiveKey, timeout);
            var _replyBuilder = new ServiceBusConnectionStringBuilder(_settings[ASBSettings.ServicebusFqdnEndpoint], ASBSettings.SessionQueueName, sasToken);
            var sessionId = Guid.NewGuid().ToString("D");

            message.ReplyToSessionId = sessionId;
            message.ReplyTo = _replyBuilder.GetEntityConnectionString();
            message.TimeToLive = timeout;

            var session = await _receiveClient.AcceptMessageSessionAsync(sessionId);

            await _sendClient.SendAsync(message);

            var reply = await session.ReceiveAsync(timeout);
            if (reply != null)
            {
                if (await replyHandler(reply))
                {
                    await session.CompleteAsync(reply.SystemProperties.LockToken);
                }
                else if (deadletterOnHandlerFailure)
                {
                    await session.DeadLetterAsync(reply.SystemProperties.LockToken);
                }
            }
            else
            {
                throw new Exception("No reply received within the timeout period");
            }
        }

        private string GenerateSasToken(string resource, string keyName, string key, TimeSpan expiresIn)
        {
            var encodedResource = HttpUtility.UrlEncode(resource);
            var expirationTime = DateTime.UtcNow.Add(expiresIn);
            var expiryTime = Convert.ToString((int)expirationTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            var unsigned = $"{encodedResource}\n{expiryTime}";
            string signature;

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                signature = HttpUtility.UrlEncode(Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsigned))));
            }

            return $"SharedAccessSignature sr={encodedResource}&sig={signature}&se={expiryTime}&skn={keyName}";
        }
    }
}
