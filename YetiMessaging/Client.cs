using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using YetiMessaging.Attrib;
using YetiMessaging.Message;
using YetiMessaging.Transport;

namespace YetiMessaging
{
	/// <summary>
	/// simple client that is used to notify server that something has happend
	/// </summary>
	public class Client : IDisposable
	{
		protected IServerTransport Transport;
		protected MessageLoader messageLoader = new MessageLoader();

		/// <summary>
		/// Initializes a new instance of the <see cref="Client"/> class.
		/// </summary>
		/// <param name="transport">The transport that should be used to touch with server.</param>
		public Client(IServerTransport transport)
		{
			Transport = transport;
		}

		public void Dispose()
		{
			try
			{
				Dispose(true);
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);
			}
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && Transport != null)
			{
				Transport.Dispose();
				Transport = null;
			}
		}

		protected byte[] idheader = new byte[] { 11, 12, 13 };

		/// <summary>
		/// Sends the specified message to the server using transport configured.
		/// </summary>
		/// <param name="message">The message.</param>
		public void Send(IMessage message)
		{
			var idattr = messageLoader.Load(message.GetType()).First();

			using (var ms = new MemoryStream())
			{
				if (idattr != null)
				{
					ms.Write(idheader, 0, idheader.Length);
					var iddata = idattr.Id.ToByteArray();
					ms.Write(iddata, 0, iddata.Length);
				}
				var data = message.Convert();
				ms.Write(data, 0, data.Length);

				Transport.Send(ms.ToArray());
			}
		}

	}
}
