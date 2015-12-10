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
	public class Client : IDisposable
	{
		protected IServerTransport Transport;
		protected AttributeLoader<IdAttribute> _loader = new AttributeLoader<IdAttribute>();

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

		public void Send(IMessage message)
		{
			var idattr = _loader.Load(message).First();

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
