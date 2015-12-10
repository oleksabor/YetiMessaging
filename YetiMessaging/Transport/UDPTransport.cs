using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace YetiMessaging.Transport
{
	public class UDPTransport : IServerTransport
	{
		/// <summary>
		/// Gets or sets the multicast host ip address. The multicast address range is 224.0.0.0 to 239.255.255.255
		/// </summary>
		/// <value>
		/// The multicast ip address.
		/// </value>
		public string Host { get; private set; }
		public int Port { get; private set; }

		UdpClient Listener;
		object listenerLock = new object();

		bool stopped;

		public Action<byte[]> OnReceive { get; set; }

		public UDPTransport(string ip, int port)
		{
			Host = ip;
			Port = port;
		}

		void Received(IAsyncResult result)
		{
			IPEndPoint remote = null;
			if (Listener != null)
				lock (listenerLock)
					if (Listener != null)
						try
						{
							var res = Listener.EndReceive(result, ref remote);
							Debug.WriteLine(string.Format("data from {0}", remote.Address), this.GetType().Name);
							if (remote != null && localAddresses.Contains(remote.Address))
							{

								if (OnReceive == null)
									Debug.WriteLine("no receiver was assigned");
								else
									OnReceive(res);
							}
							else
								Trace.TraceWarning("ignoring unknown sender");
							if (!stopped)
								Listener.BeginReceive(Received, this);
						}
						catch (Exception ex)
						{
							Trace.TraceError(string.Format("failed to receive {0}", ex.Message));
						}
			if (Listener == null)
				lock (listenerLock)
					if (Listener == null && !stopped)
					{
						Trace.TraceWarning("no active listener was found, creating a new one");
						Start();
					}

		}

		public void Start()
		{
			Listener = new UdpClient();

			Listener.ExclusiveAddressUse = false;
			IPEndPoint localEp = new IPEndPoint(IPAddress.Any, Port);

			Listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			Listener.ExclusiveAddressUse = false;

			Listener.Client.Bind(localEp);

			IPAddress multicastaddress = IPAddress.Parse(Host);
			Listener.JoinMulticastGroup(multicastaddress);

			localAddresses = NetworkInterface.GetAllNetworkInterfaces()
				.Where(_ => _.OperationalStatus == OperationalStatus.Up)
				.Select(_ => _.GetIPProperties())
				.SelectMany(_ => _.UnicastAddresses)
				.Select(_ => _.Address).ToList();

			Trace.WriteLine(string.Format("Waiting for broadcast on {0}:{1}", Host, Port), this.GetType().Name);
			var res = Listener.BeginReceive(Received, this);
		}

		IEnumerable<IPAddress> localAddresses;

		public void Stop()
		{
			stopped = true;
			if (Listener != null)
				lock (listenerLock)
					if (Listener != null)
					{
						Listener.Close();
						Listener = null;
						Trace.WriteLine(string.Format("no more listening {0}:{1}", Host, Port), this.GetType().Name);
					}
		}

		public void Dispose()
		{
			Stop();
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
