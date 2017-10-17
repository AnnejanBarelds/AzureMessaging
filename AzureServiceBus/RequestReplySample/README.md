# Azure Service Bus .NET Core Request Reply sample

This reporitory contains a sample on how to implement the Request Reply messaging pattern using Azure Service Bus and the .NET Core client library.
A sample for the .NET Framework library is available in the [official sample repo](https://github.com/Azure/azure-service-bus/tree/master/samples/DotNet/Microsoft.ServiceBus.Messaging/QueuesRequestResponse).

# Setup 

This sample makes use of the Service Bus topology also in use for the official samples; see the link above for instructions on how to setup. Make sure
to follow along until [this step](https://github.com/Azure/azure-service-bus/tree/master/samples/DotNet/Microsoft.ServiceBus.Messaging#exploring-and-running-the-samples).

Next, an extra SAS Policy named SessionQueueSend with Send permission should be created manually on the sessionqueue, which is part of the topology that was deployed during the previous step.
The key for this policy is needed in the RequestReplyHandler.cs class. Obviously, in any non-trivial context, it should be properly protected instead of hardcoded.