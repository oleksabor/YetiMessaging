using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YetiMessaging;
using YetiMessaging.Attrib;
using YetiMessaging.Message;
using YetiMessaging.Transport;

namespace YetiMessagingTest
{
	[TestFixture]
	public class EventTest
    {
		[TestCase]
		public void ListenTest()
		{
			var subscriber = new YetiMessagingTest.StringSubscriber();
			
			var transport = new UDPTransport("239.0.0.1", 2234);
			using (var service = new Server(transport))
			{
				service.Add(subscriber);
				var transport2 = new UDPTransport("239.0.0.1", 2234);
				using (var client = new Client(transport2))
				{
					client.Send(new StringMessage("testmessage"));
				}
				Thread.Sleep(1500);
				Assert.IsTrue(subscriber.Message, "no message was received");
			}
		}

		[TestCase]
		public void StringMessageTest()
		{
			const string tm = "test message";
			var sm = new StringMessage(tm);
			Assert.AreEqual(tm, sm.Value);
			
			var bytes = sm.Convert();
			sm.Deconvert(bytes);
			Assert.AreEqual(tm, sm.Value);

			sm.Deconvert(new byte[0]);
			Assert.AreEqual("", sm.Value);
		}
		
		[TestCase]
		public void IntMessageTest()
		{
			const int tm = 21;

			var im = new IntMessage(tm);
			Assert.AreEqual(tm, im.Value);

			var bytes = im.Convert();
			im.Deconvert(bytes);
			Assert.AreEqual(tm, im.Value);

			for (int q = 0; q < bytes.Length; q++)
				bytes[q] = 0;
			im.Deconvert(bytes);
			Assert.AreEqual(0, im.Value);
		}
    }

	[IdAttribute("{72F60CAB-B986-48D0-BA54-1DC4758CCD58}")]
	public class StringSubscriber : Subscriber
	{
		public bool Message { get; set; }

		public override void OnMessage(IMessage message, byte[] raw)
		{
			Trace.TraceInformation("string {0}", message.Value);
			Message = true;
		}
	}
}
