using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using YetiMessaging.Message;
using YetiMessaging.Transport;

namespace YetiMessaging
{
	/// <summary>
	/// listen client messages and notify subscribers
	/// </summary>
	public class Server : Client
	{
		//TaskFactory Tasks;

		int GuidLength = 0;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Server"/> calls subscribers in separate thread or on the same that calls <see cref="OnReceive"/>.
		/// </summary>
		/// <value>
		///   <c>true</c> if multithreaded; otherwise, <c>false</c>.
		/// </value>
		public bool Multithreaded { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Server"/> class.
		/// </summary>
		/// <param name="transport">The transport that should be used to reveive messages.</param>
		public Server(IServerTransport transport)
			: base(transport)
		{
			transport.OnReceive = OnReceive;

			_subscribers = new List<ISubscriber>();
			//Tasks = new TaskFactory();
			Start();
		}

		public void Start()
		{
			Transport.Start();
		}

		public void Stop()
		{
			Transport.Stop();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && Transport != null)
			{
				Transport.Stop();
			}
			base.Dispose(disposing);
		}

		IList<ISubscriber> _subscribers;
		object _subscribersLock = new object();

		/// <summary>
		/// Adds the specified subscriber instance.
		/// </summary>
		/// <param name="value">subscriber instance.</param>
		public void Add(ISubscriber value)
		{
			if (!_subscribers.Contains(value))
				lock (_subscribersLock)
					if (!_subscribers.Contains(value))
						_subscribers.Add(value);
		}

		IDictionary<Guid, Type> _messages;
		IDictionary<Guid, Type> Messages
		{
			get
			{
				if (_messages == null)
					lock (_subscribersLock)
						if (_messages == null)
						{
							var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(_ => _.GetTypes());
							types = types.Where(_ => typeof(IMessage).IsAssignableFrom(_) && _ != typeof(IMessage));
							_messages = new Dictionary<Guid, Type>(types.Count());
							foreach (var t in types)
								try
								{
									var attrs = _loader.Load(t);
									if (attrs.Any())
									{
										var attr = attrs.First();
										_messages.Add(attr.Id, t);

										Debug.WriteLine(string.Format("known message {0} {1}", attr.Id, t.Name), this.GetType().Name);
									}
									else
										Trace.TraceWarning("no IdAttribute on {0}", t);

								}
								catch { }
						}
				return _messages;
			}
		}


		protected virtual void OnReceive(byte[] value)
		{
			if (Multithreaded)
				System.Threading.ThreadPool.QueueUserWorkItem(_ => OnReceiveSingle(value));
			else
				OnReceiveSingle(value);
		}

		/// <summary>
		/// Called when message is received with transport. Parses message, tries to find appropriate subscriber
		/// </summary>
		/// <param name="value">The raw message value.</param>
		protected virtual void OnReceiveSingle(byte[] value)
		{
			try
			{
				IEnumerable<ISubscriber> notifications = _subscribers;
				IMessage message = null;
				bool headerFound = true;
				if (value.Length > idheader.Length)
				{
					var idbytes = new byte[idheader.Length];
					if (idheader.SequenceEqual(value.Take(idheader.Length)))
					{
						GuidLength = GuidLength == 0 ? Guid.NewGuid().ToByteArray().Length : GuidLength;
						var idguid = new Guid(value.Skip(idheader.Length).Take(GuidLength).ToArray());

						value = value.Skip(idheader.Length + GuidLength).ToArray();
						notifications = _subscribers.Where(_ => _.Matches(idguid));
						if (notifications.Count() == 0)
						{
							Debug.WriteLine(string.Format("no subscriber was found for {0}", idguid), this.GetType().Name);
							foreach (var t in _subscribers)
								Debug.WriteLine(string.Format("subscribed {0} {1}", string.Join(",", t.Ids.Select(_ => _.ToString()).ToArray()), t.GetType().Name), this.GetType().Name);
							Debug.WriteLine(".", this.GetType().Name);
						}
						var messageType = Messages[idguid];
						message = (IMessage)Activator.CreateInstance(messageType);
						message.Deconvert(value);
					}
				}
				else
				{
					headerFound = false;
					Debug.WriteLine("no idheader in message was found");
				}

				lock (_subscribersLock)
					notifications = notifications.ToList();
				foreach (var notify in notifications)
					try
					{
						Debug.WriteLine(string.Format("trying notify {0}", notify.GetType().Name), this.GetType().Name); 
						notify.OnMessage(message, value);
					}
					catch (Exception ex)
					{
						Trace.WriteLine(string.Format("failed to notify {0}", notify.GetType().Name), this.GetType().Name);
						if (headerFound)
						{
							Trace.TraceError(ex.Message);
							lock (_subscribersLock)
								_subscribers.Remove(notify);
						}
					}
			}
			catch (Exception ex)
			{
				Trace.WriteLine(string.Format("failed to OnReceiveSingle '{0}'", ex.Message), this.GetType().Name);
			}
		}
	}
}
