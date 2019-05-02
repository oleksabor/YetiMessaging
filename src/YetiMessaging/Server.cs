using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using YetiMessaging.Attrib;
using YetiMessaging.Logging;
using YetiMessaging.Message;
using YetiMessaging.Transport;

namespace YetiMessaging
{
	/// <summary>
	/// listen client messages and notify subscribers
	/// </summary>
	public class Server : Client
	{
		ILog Log = LogProvider.GetCurrentClassLogger();

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
			: this(transport, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Server"/> class.
		/// </summary>
		/// <param name="transport">The transport that should be used to reveive messages.</param>
		public Server(IServerTransport transport, Func<Type, IMessage> factory)
			: base(transport)
		{
			messageFactory = factory;

			transport.OnReceive = OnReceive;

			_subscribers = new List<ISubscriber>();
			//Tasks = new TaskFactory();
			Start();
		}

		readonly Func<Type, IMessage> messageFactory;

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
							_messages = messageLoader.GetMessages();
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

		public IMessage CreateMessage(Type messageType)
		{
			return messageFactory?.Invoke(messageType) ?? (IMessage)Activator.CreateInstance(messageType);
		}

		protected IMessage Deconvert(byte[] value)
		{
			IMessage message = null;
			if (value.Length > idheader.Length)
			{
				var idbytes = new byte[idheader.Length];
				if (idheader.SequenceEqual(value.Take(idheader.Length)))
				{
					GuidLength = GuidLength == 0 ? Guid.NewGuid().ToByteArray().Length : GuidLength;
					var idguid = new Guid(value.Skip(idheader.Length).Take(GuidLength).ToArray());

					value = value.Skip(idheader.Length + GuidLength).ToArray();

					var messageType = Messages[idguid];
					message = CreateMessage(messageType);
					message.Deconvert(value);
				}
			}
			return message;
		}

		void Notify(IMessage message, byte[] value, IEnumerable<ISubscriber> notifications)
		{
			foreach (var notify in notifications)
				try
				{
					Log.Debug("trying notify {0}", notify.GetType().Name);
					notify.OnMessage(message, value);
					if (message.Handled)
						break;
				}
				catch (Exception ex)
				{
					Log.Error(ex, "failed to notify {0}", notify.GetType().Name);
					Unsubscribe(notify);
				}
		}

		void Unsubscribe(ISubscriber value)
		{
			lock (_subscribersLock)
				_subscribers.Remove(value);
		}

		protected virtual void OnReceiveSingle(byte[] value)
		{
			try
			{
				IEnumerable<ISubscriber> notifications = _subscribers;
				IMessage message = Deconvert(value);
				if (message == null)
				{
					Log.Warn("no message was deconverted");
					return;
				}
				var attrLoader = new MessageLoader();
				notifications = _subscribers.Where(_ => _.Matches(attrLoader.Load(message.GetType()).First().Id));
				lock (_subscribersLock)
					notifications = notifications.ToList();

				Notify(message, value, notifications);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "failed to OnReceiveSingle");
			}
		}
	}
}
