using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YetiMessaging;
using YetiMessaging.Message;
using YetiMessaging.Transport;

namespace YetiMessagingTest
{
	[TestFixture]
	public class ServerTest
	{
		[TestCase]
		public void CreateFactory()
		{
			var transport = MockRepository.GenerateMock<IServerTransport>();

			var factory = new Func<Type, IMessage>(t => new LoaderMessage());

			var server = new Server(transport, factory);

			Assert.IsInstanceOf<LoaderMessage>(server.CreateMessage(MockRepository.GenerateMock<IMessage>().GetType()));
		}

		[TestCase]
		public void CreateActivator()
		{
			var transport = MockRepository.GenerateMock<IServerTransport>();

			var server = new Server(transport);

			var messageType = typeof(LoaderMessage);

			Assert.IsInstanceOf<LoaderMessage>(server.CreateMessage(messageType));
		}

	}
}
