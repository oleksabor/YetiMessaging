using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YetiMessaging.Attrib
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class IdAttribute : Attribute
	{
		public Guid Id; 
		public IdAttribute(string guidId)
		{
			Id = new Guid(guidId);
		}

		public override object TypeId
		{
			get
			{
				return Id.GetHashCode();
			}
		}
	}
}
