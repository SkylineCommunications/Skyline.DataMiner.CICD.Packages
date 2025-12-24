using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	public class InvalidProfileParameterValueException : Exception
	{
		public InvalidProfileParameterValueException()
		{
		}

		public InvalidProfileParameterValueException(string message)
			: base(message)
		{
		}

		public InvalidProfileParameterValueException(string profileParameterName, object value)
			: base($"{value} is an invalid value for Profile Parameter {profileParameterName}")
		{
		}

		public InvalidProfileParameterValueException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}