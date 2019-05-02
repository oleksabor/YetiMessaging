using System;
using System.Runtime.Serialization;

namespace YetiMessaging.Transport
{
	public class TransportException : ApplicationException
	{
		public TransportException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public TransportException(string message)
			: base(message)
		{
		}

		public TransportException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
