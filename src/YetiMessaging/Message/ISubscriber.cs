using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YetiMessaging.Message
{
	public interface ISubscriber
	{
		void OnMessage(IMessage message, byte[] raw);
		bool Matches(Guid value);

		IEnumerable<Guid> Ids { get; }
	}
}
