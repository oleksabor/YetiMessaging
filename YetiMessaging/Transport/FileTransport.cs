using System;
using System.Diagnostics;
using System.IO;

namespace YetiMessaging.Transport
{
	public class FileTransport : IServerTransport, IDisposable
	{
		private FileSystemWatcher _watcher;

		private string FileName;

		private object _fileLock = new object();

		public Action<byte[]> OnReceive { get; set; }

		public FileTransport(string fileName)
		{
			string text = Environment.ExpandEnvironmentVariables(fileName);
			if (!Directory.Exists(Path.GetDirectoryName(text)))
			{
				throw new DirectoryNotFoundException(string.Format("no file exists {0} {1}", fileName, (fileName == text) ? "" : text));
			}
			this.FileName = text;
		}

		public void Stop()
		{
			if (this._watcher != null)
			{
				this._watcher.Changed -= new FileSystemEventHandler(this._watcher_Changed);
				this._watcher.Dispose();
				this._watcher = null;
				Trace.WriteLine(string.Format("watcher disposed {0}", this.FileName), base.GetType().Name);
			}
		}

		public void Start()
		{
			this._watcher = new FileSystemWatcher(Path.GetDirectoryName(this.FileName), Path.GetFileName(this.FileName));
			this._watcher.Changed += new FileSystemEventHandler(this._watcher_Changed);
			this._watcher.NotifyFilter = NotifyFilters.LastWrite;
			this._watcher.EnableRaisingEvents = true;
			Trace.WriteLine(string.Format("watching in {0} for {1}", this._watcher.Path, this._watcher.Filter), base.GetType().Name);
		}

		private void _watcher_Changed(object sender, FileSystemEventArgs e)
		{
			Trace.WriteLine(string.Format("got _watcher_Changed {0} {1}", e.ChangeType, e.FullPath), base.GetType().Name);
			byte[] obj = null;
			lock (this._fileLock)
			{
				obj = File.ReadAllBytes(e.FullPath);
			}
			if (this.OnReceive != null)
			{
				this.OnReceive(obj);
			}
		}

		public void Send(byte[] data)
		{
			Trace.WriteLine(string.Format("notifying {0}", this.FileName), base.GetType().Name);
			lock (this._fileLock)
			{
				File.WriteAllBytes(this.FileName, data);
			}
		}

		public void Dispose()
		{
			this.Stop();
		}
	}
}
