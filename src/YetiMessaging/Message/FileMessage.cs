using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YetiMessaging.Attrib;

namespace YetiMessaging.Message
{
	[Id("{825C5FEC-76D7-4BE8-815C-DE1F9389D3CE}")]
	public class FileMessage : StringMessage
	{
		public FileMessage()
		{ }

		public FileMessage(string message)
			: base(message)
		{
		}
	}
}
