using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using YetiMessaging.Attrib;

namespace YetiMessaging.Message
{
	[IdAttribute("{72F60CAB-B986-48D0-BA54-1DC4758CCD58}")]
	public class StringMessage : IMessage
	{
		protected string _value;

		public StringMessage()
		{ }

		public StringMessage(string value)
		{
			_value = value;
		}

		public virtual byte[] Convert()
		{
			return Encoding.ASCII.GetBytes(_value ?? "");
		}

		public virtual void Deconvert(byte[] data)
		{
			_value = Encoding.ASCII.GetString(data);
		}

		public object Value { get { return _value; } }

		public bool Handled { get; set; }
	}
}
