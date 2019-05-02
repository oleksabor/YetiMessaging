using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YetiMessaging.Message
{
	public interface IMessage
	{
		void Deconvert(byte[] data);
		byte[] Convert();

		object Value { get; }

		bool Handled { get; set; }
	}
}
