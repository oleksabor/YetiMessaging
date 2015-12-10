using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YetiMessaging;
using YetiMessaging.Message;
using YetiMessaging.Transport;

namespace YetiMessagingTest
{
	[TestFixture]
	public class FileTest
	{
		[TestCase]
		public void MissingFileTest()
		{
			Assert.Catch<DirectoryNotFoundException>(MissingPathActualTest);
		}

		public void MissingPathActualTest()
		{
			using (var fm = new FileTransport("%temp%\\nonexisting\\file.txt"))
			{
				Trace.TraceWarning("never running line");
			}
		}

		[TestCase]
		public void FileServerTest()
		{
			var fileName = @"d:\filetransport.txt";
			using (var fm = new Server(new FileTransport(fileName)))
			{
				fm.Add(new FileSubscriber());

				using (var cln = new Client(new FileTransport(fileName)))
					cln.Send(new FileMessage("test file line"));


				Thread.Sleep(1000);
			}
		}
	}

	[YetiMessaging.Attrib.Id("{825C5FEC-76D7-4BE8-815C-DE1F9389D3CE}")]
	public class FileSubscriber : Subscriber
	{
		public override void OnMessage(IMessage message, byte[] raw)
		{
			Trace.WriteLine(string.Format("got message {0}", message), this.GetType().Name);
		}
	}
}
