﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YetiMessaging.Attrib;

namespace YetiMessaging.Message
{
	[IdAttribute("{6FEF4D36-6637-4228-8758-5C296E373DFA}")]
	public class IntMessage : IMessage
	{
		int _value;

		public IntMessage()
		{ }

		public IntMessage(int value)
		{
			_value = value;
		}

		public void Deconvert(byte[] data)
		{
			_value = BitConverter.ToInt32(data, 0);
		}

		public byte[] Convert()
		{
			return BitConverter.GetBytes(_value);
		}

		public object Value
		{
			get { return _value; }
		}

		public bool Handled { get; set; }
	}
}
