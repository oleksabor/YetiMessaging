using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YetiMessaging.Transport
{
	public interface IServerTransport : IDisposable
	{
		void Stop();
		void Start();

		void Send(byte[] data);

		Action<byte[]> OnReceive { get; set; }
	}
}
