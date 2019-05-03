YetiMessaging is very simple and lightweight framework that allows to publish and consume short broadcast messages on the certain machine.

Different transports can be used, UDP (no state connection and no guarantee) and file message (for fun) transports were implemented.Server can be parameterized to use outer message factory.Message has to implement custom `IMessage` interface and to be marked using `Id` attribute.

I have used YetiMessaging for VisualStudio addin to inform all addin instances about configuration changes.

Test cases can be found in the YetiMessagingTest folder

Simplest one is below

````
var subscriber = new YetiMessagingTest.StringSubscriber();

var transport = new UDPTransport("239.0.0.1", 2234);
using (var service = new Server(transport)) //server is ready and listening
{
	service.Add(subscriber); // subscriber has OnMessage method that is called with service if message type matches with subscriber type
	
	var transport2 = new UDPTransport("239.0.0.1", 2234); //client initialization, please note that udp transport instance can not be shared between client and service
	using (var client = new Client(transport2))
	{
		client.Send(new StringMessage("testmessage"));
	}
	
	Thread.Sleep(1500); // server should have time to get message from network and parse it
	Assert.IsTrue(subscriber.Message, "no message was received");
}
````

Here is sample subscriber implementation
````
[IdAttribute("{72F60CAB-B986-48D0-BA54-1DC4758CCD58}")]
public class StringSubscriber : Subscriber
{
	public bool Message { get; set; }

	public override void OnMessage(IMessage message, byte[] raw)
	{
		Trace.TraceInformation("string {0}", message.Value);
		Message = true;
	}
}````

Subscriber type is set with Id attribute. It should have the same value that was set on appropriate message type. Please find string message base implementation below````
[IdAttribute("{72F60CAB-B986-48D0-BA54-1DC4758CCD58}")]
public class StringMessage : IMessage
{
}````
Message and subscriber has to be marked using the same GUID value. When message is going to be sent to listeners GUID value is saved to the stream before `IMessage.Convert()` serializes the message.When received server checks by GUID value is there subscriber for this kind of message or not. 


