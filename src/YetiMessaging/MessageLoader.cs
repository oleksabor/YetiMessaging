using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YetiMessaging.Attrib;
using YetiMessaging.Logging;
using YetiMessaging.Message;

namespace YetiMessaging
{
	public class MessageLoader
	{
		ILog Log = LogProvider.GetCurrentClassLogger();

		AttributeLoader<IdAttribute> loader = new AttributeLoader<IdAttribute>();

		IEnumerable<string> ignoreFiles = new Collection<string> { @"^NUnit", @"^System", @"^Microsoft" };
		IEnumerable<Regex> ignoreRegexes = new Collection<Regex>();

		public MessageLoader()
		{
			ignoreRegexes = ignoreFiles.Select(_ => new Regex(_, RegexOptions.IgnoreCase));
		}

		public IDictionary<Guid, Type> GetMessages()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var assemblies = GetAssemblies(path);

			var types = assemblies.SelectMany(_ => GetAssemblyTypes(_));
			return GetMessages(types);
		}

		public IDictionary<Guid, Type> GetMessages(IEnumerable<Type> types)
		{
			types = types.Where(_ => typeof(IMessage).IsAssignableFrom(_) && _.IsClass && !_.IsAbstract);
			var res = new Dictionary<Guid, Type>(types.Count());
			foreach (var t in types)
				try
				{
					var attrs = loader.Load(t);
					if (attrs.Any())
					{
						var attr = attrs.First();
						res.Add(attr.Id, t);

						Log.Info("known message {0} {1}", attr.Id, t.Name);
					}
					else
						Log.Warn("no IdAttribute on {0}", t);

				}
				catch (ReflectionTypeLoadException)
				{
					Log.Warn("failed to load type {0}", t);
				}
			return res;
		}

		IEnumerable<Assembly> GetAssemblies(string path)
		{
			var assemblies = new List<Assembly>();
			foreach (var f in Directory.GetFiles(path))
				if (CanLoad(f))
					try
					{
						assemblies.Add(Assembly.LoadFrom(f));
						Log.Debug(f);
					}
					catch (Exception)
					{
						Log.Warn("failed to load {0}", f);
					}
			return assemblies;
		}

		IEnumerable<Type> GetAssemblyTypes(Assembly a)
		{
			try
			{
				return a.GetExportedTypes();
			}
			catch (Exception e)
			{
				Log.WarnException("failed to get assembly types", e);
				return new Collection<Type>();
			}
		}

		public IEnumerable<IdAttribute> Load(Type source)
		{
			return loader.Load(source);
		}

		StringIgnoreCase ignoreCase = new StringIgnoreCase();

		public bool CanLoad(string f)
		{
			var ext = Path.GetExtension(f);
			var file = Path.GetFileName(f);
			return (ignoreCase.Equals(".dll", ext) || ignoreCase.Equals(".exe", ext)) 
				&& !ignoreRegexes.Any(_ => _.IsMatch(file));
		}

		class StringIgnoreCase : IEqualityComparer<string>
		{
			public bool Equals(string x, string y)
			{
				return x.Equals(y, StringComparison.OrdinalIgnoreCase);
			}

			public int GetHashCode(string obj)
			{
				throw new NotImplementedException();
			}
		}
	}
}
