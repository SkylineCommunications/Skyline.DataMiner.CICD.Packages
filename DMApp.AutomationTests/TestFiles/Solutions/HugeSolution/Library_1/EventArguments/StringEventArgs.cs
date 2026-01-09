using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library_1.EventArguments
{
	public class StringEventArgs : EventArgs
	{
		public StringEventArgs(string arg)
		{
			Value = arg;
		}

		public string Value { get; }
	}
}
