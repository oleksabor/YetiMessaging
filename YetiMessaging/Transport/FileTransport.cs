using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace YetiMessaging.Transport
{
	public class FileTransport : IServerTransport
	{
		FileSystemWatcher _watcher;
		string FileName;

		object _fileLock = new object();

		public FileTransport(string fileName)
		{
			var fn = Environment.ExpandEnvironmentVariables(fileName);
			if (!Directory.Exists(Path.GetDirectoryName(fn)))
				throw new DirectoryNotFoundException(string.Format("no file exists {0} {1}", fileName, fileName == fn ? "" : fn));
			FileName = fn;
		}

		public void Stop()
		{
			if (_watcher != null)
			{
				_watcher.Changed -= _watcher_Changed;
				_watcher.Dispose();
				_watcher = null;
				Trace.WriteLine(string.Format("watcher disposed {0}", FileName), this.GetType().Name);
			}
		}

		public void Start()
		{
			_watcher = new FileSystemWatcher(Path.GetDirectoryName(FileName), Path.GetFileName(FileName));
			_watcher.Changed += _watcher_Changed;
			_watcher.NotifyFilter = NotifyFilters.LastWrite;
			_watcher.EnableRaisingEvents = true;

			Trace.WriteLine(string.Format("watching in {0} for {1}", _watcher.Path, _watcher.Filter), this.GetType().Name);
		}

		void _watcher_Changed(object sender, FileSystemEventArgs e)
		{
			Trace.WriteLine(string.Format("got _watcher_Changed {0} {1}", e.ChangeType, e.FullPath), this.GetType().Name);
			byte[] buffer = null;
			lock (_fileLock)
			{
				buffer = File.ReadAllBytes(e.FullPath);
			}
			if (OnReceive != null)
			{
				OnReceive(buffer);
			}
		}

		public void Send(byte[] data)
		{
			Trace.WriteLine(string.Format("notifying {0}", FileName), this.GetType().Name);
			lock (_fileLock)
			{
				File.WriteAllBytes(FileName, data);
			}
		}

		public Action<byte[]> OnReceive { get; set; }

		public void Dispose()
		{
			Stop();
		}
	}
}
