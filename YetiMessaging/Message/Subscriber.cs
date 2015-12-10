using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using YetiMessaging.Attrib;

namespace YetiMessaging.Message
{
	public abstract class Subscriber : ISubscriber
	{
		public abstract void OnMessage(IMessage message, byte[] raw);

		AttributeLoader<IdAttribute> _loader = new AttributeLoader<IdAttribute>();

		public bool Matches(Guid value)
		{
			var ids = _loader.Load(this);
			var res = ids.Any(_ => _.Id == value);
			if (!res)
			{
				if (ids.Count() > 0)
					foreach (var i in ids)
						Debug.WriteLine(string.Format("subs {0} {1}", this.GetType().Name, i.Id));
				else
					Debug.WriteLine(string.Format("no custom id attr on {0}", this.GetType().Name));
			}
			return res; 
		}

		public IEnumerable<Guid> Ids
		{
			get { return _loader.Load(this).Select(_ => _.Id); }
		}
	}
}
