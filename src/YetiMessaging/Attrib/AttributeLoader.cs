using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YetiMessaging.Attrib
{
	public class AttributeLoader<T> where T : Attribute
	{
		IDictionary<Type, IEnumerable<T>> _cache = new Dictionary<Type, IEnumerable<T>>();

		object _cacheLock = new object();

		public IEnumerable<T> Load(object source)
		{
			return Load(source.GetType());
		}

		public IEnumerable<T> Load(Type source)
		{
			IEnumerable<T> res = null;
			if (!_cache.TryGetValue(source, out res))
				lock (_cacheLock)
					if (!_cache.TryGetValue(source, out res))
					{
						var attrs = source.GetCustomAttributes(typeof(T), false);
						if (attrs.Length == 0)
							attrs = source.GetCustomAttributes(typeof(T), true);
						res = attrs.OfType<T>();
						_cache.Add(source, res);
					}
			return res;
		}
	}
}
