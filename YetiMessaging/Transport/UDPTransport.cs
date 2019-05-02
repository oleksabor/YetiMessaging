using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using YetiMessaging.Logging;

namespace YetiMessaging.Transport
{
	public class UDPTransport : IServerTransport, IDisposable
	{
		ILog Log = LogProvider.GetCurrentClassLogger();

		/// <summary>
		/// Gets or sets the multicast host ip address. The multicast address range is 224.0.0.0 to 239.255.255.255
		/// </summary>
		/// <value>
		/// The multicast ip address.
		/// </value>
		public string Host { get; private set; }
		public int Port { get; private set; }

		UdpClient Listener;
		private object listenerLock = new object();

		private bool stopped;

		private IEnumerable<IPAddress> localAddresses;

		public Action<byte[]> OnReceive { get; set; }

		public UDPTransport(string ip, int port)
		{
			this.Host = ip;
			this.Port = port;
		}

		private void Received(IAsyncResult result)
		{
			if (this.Listener != null)
			{
				lock (this.listenerLock)
				{
					if (this.Listener != null)
					{
						try
						{
							byte[] data = ReceiveFromLocal(this.Listener, result);

							if (data != null && this.OnReceive != null)
									this.OnReceive(data);

							if (!this.stopped)
								this.Listener.BeginReceive(new AsyncCallback(this.Received), this);
						}
						catch (Exception ex)
						{
							Log.Error("failed to receive {0}", ex.Message);
						}
					}
				}
			}
			if (this.Listener == null)
			{
				lock (this.listenerLock)
				{
					if (this.Listener == null && !this.stopped)
					{
						Trace.TraceWarning("no active listener was found, creating a new one");
						this.Start();
					}
				}
			}

			byte[] ReceiveFromLocal(UdpClient listener, IAsyncResult res)
			{
				IPEndPoint iPEndPoint = null;
				byte[] obj2 = listener.EndReceive(res, ref iPEndPoint);
				if (iPEndPoint != null && this.localAddresses.Contains(iPEndPoint.Address))
					return obj2;
				else
				{
					Log.Warn("ignoring unknown sender '{0}'", iPEndPoint?.Address);
					return null;
				}
			}
		}

		public void Start()
		{
			try
			{
				this.Listener = new UdpClient();
				this.Listener.ExclusiveAddressUse = false;
				IPEndPoint localEP = new IPEndPoint(IPAddress.Any, this.Port);

				this.Listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				this.Listener.ExclusiveAddressUse = false;
				this.Listener.Client.Bind(localEP);

				IPAddress multicastAddr = IPAddress.Parse(this.Host);
				this.Listener.JoinMulticastGroup(multicastAddr);

				this.localAddresses = NetworkInterface.GetAllNetworkInterfaces()
					.Where(_ => _.OperationalStatus == OperationalStatus.Up)
					.Select(_ => _.GetIPProperties())
					.SelectMany(_ => _.UnicastAddresses)
					.Select(_ => _.Address).ToList();

				Log.Debug("Waiting for broadcast on {0}:{1}", this.Host, this.Port);
				this.Listener.BeginReceive(new AsyncCallback(this.Received), this);
			}
			catch (Exception inner)
			{
				throw new TransportException(string.Format("failed to start transport {0} {1}", this.Host, this.Port), inner);
			}
		}

		public void Stop()
		{
			this.stopped = true;
			if (this.Listener != null)
			{
				lock (this.listenerLock)
				{
					if (this.Listener != null)
					{
						this.Listener.Close();
						this.Listener = null;
						Log.Debug("no more listening {0}:{1}", this.Host, this.Port);
					}
				}
			}
		}

		public void Dispose()
		{
			this.Stop();
		}

		public void Send(byte[] data)
		{
			using (var udpclient = new UdpClient())
			{
				IPAddress multicastaddress = IPAddress.Parse(Host);
				udpclient.JoinMulticastGroup(multicastaddress);
				IPEndPoint remoteep = new IPEndPoint(multicastaddress, Port);

				var sent = udpclient.Send(data, data.Length, Host, Port);
				Debug.Assert(data.Length == sent, "somethig wrong to sent");
			}
		}
	}
}
