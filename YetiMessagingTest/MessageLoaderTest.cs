using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YetiMessaging;
using YetiMessaging.Attrib;
using YetiMessaging.Message;

namespace YetiMessagingTest
{
	[TestFixture]
	public class MessageLoaderTest
	{
		[TestCase]
		public void LoadMessageType()
		{
			var loader = new MessageLoader();
			var types = new[] { typeof(MessageLoaderTest), typeof(LoaderMessage) };
			var messages = loader.GetMessages(types);
			Assert.AreEqual(1, messages.Count());
		}

		[TestCase]
		public void CanLoad()
		{
			var loader = new MessageLoader();

			Assert.IsTrue(loader.CanLoad("asdfasdf.System.dll"));
			Assert.IsFalse(loader.CanLoad("asdfasdf.System.pdb"));

			Assert.IsTrue(loader.CanLoad("asdfasdf.Microsoft.dll"));
			Assert.IsFalse(loader.CanLoad("asdfasdf.Microsoft.pdb"));

			Assert.IsFalse(loader.CanLoad("nunit.dll"));
			Assert.IsFalse(loader.CanLoad("nunit3.testadapter.dll"));
			Assert.IsFalse(loader.CanLoad("System.Serialization.dll"));
		}

		[TestCase]
		public void CanLoadWithPath()
		{
			var loader = new MessageLoader();

			Assert.IsTrue(loader.CanLoad("c:\\data\\asdfasdf.System.dll"));
			Assert.IsFalse(loader.CanLoad("c:\\data\\asdfasdf.System.pdb"));

			Assert.IsTrue(loader.CanLoad("c:\\data\\asdfasdf.Microsoft.dll"));
			Assert.IsFalse(loader.CanLoad("c:\\data\\asdfasdf.Microsoft.pdb"));

			Assert.IsFalse(loader.CanLoad("c:\\data\\nunit.dll"));
			Assert.IsFalse(loader.CanLoad("c:\\data\\nunit3.testadapter.dll"));
			Assert.IsFalse(loader.CanLoad("c:\\data\\System.Serialization.dll"));
		}
	}

	[IdAttribute("FED60C00-3C75-4DEA-B002-CAD73C07531E")]
	public class LoaderMessage : IMessage
	{
		public object Value => throw new NotImplementedException();

		public bool Handled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public byte[] Convert()
		{
			throw new NotImplementedException();
		}

		public void Deconvert(byte[] data)
		{
			throw new NotImplementedException();
		}
	}
}
