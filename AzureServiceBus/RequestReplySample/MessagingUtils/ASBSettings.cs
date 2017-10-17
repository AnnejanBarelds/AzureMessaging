/* This class is a modified version from some code in the official ASB sample repo at https://github.com/Azure/azure-service-bus/tree/master/samples/DotNet/Microsoft.ServiceBus.Messaging
 * All samples in that repo, as well as this class, depend a specific setup described in the repo's Readme, up to and including https://github.com/Azure/azure-service-bus/tree/master/samples/DotNet/Microsoft.ServiceBus.Messaging#exploring-and-running-the-samples */

 using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MessagingUtils
{
    public class ASBSettings
    {
        public static readonly string BasicQueueName = "BasicQueue";
        public static readonly string PartitionedQueueName = "PartitionedQueue";
        public static readonly string DupdetectQueueName = "DupdetectQueue";
        public static readonly string BasicTopicName = "BasicTopic";
        public static readonly string SendKeyName = "samplesend";
        public static readonly string ReceiveKeyName = "samplelisten";
        public static readonly string SessionQueueName = "SessionQueue";
        public static readonly string BasicQueue2Name = "BasicQueue2";
        public static readonly string ManageKeyName = "samplemanage";
        public static readonly string ServicebusNamespace = "SERVICEBUS_NAMESPACE";
        public static readonly string ServicebusFqdnEndpoint = "SERVICEBUS_FQDN_ENDPOINT";
        public static readonly string ServicebusSendKey = "SERVICEBUS_SEND_KEY";
        public static readonly string ServicebusListenKey = "SERVICEBUS_LISTEN_KEY";
        public static readonly string ServicebusManageKey = "SERVICEBUS_MANAGE_KEY";
        static readonly string samplePropertiesFileName = "azure-msg-config.properties";

        private IDictionary<string, string> _properties;

        public string this[string key]
        {
            get
            {
                return _properties[key];
            }
        }

        public ASBSettings()
        {
            _properties = new Dictionary<string, string>
            {
                {ServicebusNamespace, null},
                {ServicebusFqdnEndpoint, null},
                {ServicebusSendKey, null},
                {ServicebusListenKey, null},
                {ServicebusManageKey, null}
            };

            // read the settings file created by the ./setup.ps1 file
            var settingsFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                samplePropertiesFileName);
            if (File.Exists(settingsFile))
            {
                using (var fs = new StreamReader(settingsFile))
                {
                    while (!fs.EndOfStream)
                    {
                        var readLine = fs.ReadLine();
                        if (readLine != null)
                        {
                            var propl = readLine.Trim();
                            var cmt = propl.IndexOf('#');
                            if (cmt > -1)
                            {
                                propl = propl.Substring(0, cmt).Trim();
                            }
                            if (propl.Length > 0)
                            {
                                var propi = propl.IndexOf('=');
                                if (propi == -1)
                                {
                                    continue;
                                }
                                var propKey = propl.Substring(0, propi).Trim();
                                var propVal = propl.Substring(propi + 1).Trim();
                                if (_properties.ContainsKey(propKey))
                                {
                                    _properties[propKey] = propVal;
                                }
                            }
                        }
                    }
                }
            }

            // get overrides from the environment
            foreach (var prop in _properties.Keys.ToArray())
            {
                var env = Environment.GetEnvironmentVariable(prop);
                if (env != null)
                {
                    _properties[prop] = env;
                }
            }

            var endpoint = new Uri(_properties[ServicebusFqdnEndpoint]);
            var hostName = endpoint.Host;
            var sbUri = new UriBuilder("sb", hostName, -1, "/").ToString();
            _properties[ServicebusFqdnEndpoint] = sbUri;
        }
    }
}
